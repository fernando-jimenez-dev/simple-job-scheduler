using Application.Shared.Errors;
using Application.UseCases.ExecutePowerShell.Errors;
using FluentResults;
using System.Net;
using System.Text.Json.Serialization;
using WebAPI.Minimal.Shared;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace WebAPI.Minimal.UseCases.ExecutePowerShell;

public record ExecutePowerShellResponse
{
    public bool IsSuccess { get; init; }
    public ExecutePowerShellOutput? Data { get; init; }
    public ApiError? Error { get; init; }
    public HttpStatusCode HttpStatusCode { get; init; }

    [JsonIgnore]
    public int StatusCode => (int)HttpStatusCode;

    public ExecutePowerShellResponse(Result<ExecutePowerShellOutput> useCaseResult, ExecutePowerShellRequest request)
    {
        if (useCaseResult.IsSuccess)
        {
            IsSuccess = true;
            Data = useCaseResult.Value;
            HttpStatusCode = HttpStatusCode.OK;
            return;
        }

        IsSuccess = false;
        Data = null;

        var useCaseError = useCaseResult.Errors.First();
        (HttpStatusCode, Error) = MapErrorToHttp(useCaseError, request);
    }

    /// <param name="useCaseError">
    /// It should ALWAYS be a <see cref="ApplicationError"/> type.
    /// Use Cases MUST NOT return a FluentResults.Error, instead they return a proper custom model ApplicationError.
    /// And every custom Error type must inherit from ApplicationError, not FluentResults.Error.
    /// I have to evaluate if it would be worth replacing FluentResults.Result with a custom ApplicationResult -
    /// making the contract less dependant on a third-party model, which I hate right now but whatever.
    /// </param>
    private static (HttpStatusCode, ApiError) MapErrorToHttp(IError useCaseError, ExecutePowerShellRequest request)
    {
        return useCaseError switch
        {
            FailureExitCodeError error => (
                HttpStatusCode.UnprocessableEntity,
                ApiError.FromApplicationError(error, request)
                    .WithDetail("exitCode", error.ExitCode)
                    .WithDetail("stdout", error.StandardOutput)
                    .WithDetail("stderr", error.StandardError)
            ),

            FileNotFoundError error => (
                HttpStatusCode.NotFound,
                ApiError.FromApplicationError(error, request)
            ),

            FileIsNotPowerShellError error => (
                HttpStatusCode.BadRequest,
                ApiError.FromApplicationError(error, request)
            ),

            InvalidInputError error => (
                HttpStatusCode.BadRequest,
                ApiError.FromApplicationError(error, request)
                    .WithDetail("validationMessage", error.ValidationMessage)
            ),

            UnexpectedError error => (
                HttpStatusCode.InternalServerError,
                ApiError.FromApplicationError(error, request)
            ),

            ApplicationError unclasifiedError => (
                HttpStatusCode.InternalServerError,
                ApiError.FromApplicationError(unclasifiedError, request)
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiError.FromApplicationError(new UnexpectedError("An unexpected error ocurred."), request)
            ),
        };
    }
}