using Application.Shared.Errors;
using Application.UseCases.CreateJobSchedule;
using Application.UseCases.CreateJobSchedule.Errors;
using FluentResults;
using System.Net;
using WebAPI.Minimal.Shared;

namespace WebAPI.Minimal.UseCases.CreateJobSchedule;

public record CreateJobScheduleResponse
{
    public bool IsSuccess { get; init; }
    public CreateJobScheduleOutput? Data { get; init; }
    public ApiError? Error { get; init; }
    public HttpStatusCode HttpStatusCode { get; init; }
    public int StatusCode => (int)HttpStatusCode;

    public const HttpStatusCode SuccessCode = HttpStatusCode.Created;
    public const HttpStatusCode ValidationErrorCode = HttpStatusCode.BadRequest;
    public const HttpStatusCode JobDoesNotExistErrorCode = HttpStatusCode.NotFound;
    public const HttpStatusCode FailedToSaveScheduleErrorCode = HttpStatusCode.InternalServerError;

    public CreateJobScheduleResponse(Result<CreateJobScheduleOutput> useCaseResult, CreateJobScheduleRequest request)
    {
        if (useCaseResult.IsSuccess)
        {
            IsSuccess = true;
            Data = useCaseResult.Value;
            HttpStatusCode = SuccessCode;
            return;
        }

        IsSuccess = false;
        Data = null;

        var useCaseError = useCaseResult.Errors.First();
        (HttpStatusCode, Error) = MapErrorToHttp(useCaseError, request);
    }

    private static (HttpStatusCode, ApiError) MapErrorToHttp(IError useCaseError, CreateJobScheduleRequest request)
    {
        return useCaseError switch
        {
            ValidationError<CreateJobScheduleInput> error => (
                ValidationErrorCode,
                ApiError.FromApplicationError(error, request)
                    .WithDetail("validationIssues", error.Issues)
            ),
            JobDoesNotExistError error => (
                JobDoesNotExistErrorCode,
                ApiError.FromApplicationError(error, request)
                    .WithDetail("jobId", error.JobId)
            ),
            FailedToSaveScheduleError error => (
                FailedToSaveScheduleErrorCode,
                ApiError
                    .FromApplicationError(error, request)
                    .WithDetail(
                        "repositoryError",
                        error.SaveScheduleResult.Errors.FirstOrDefault()?.Message ?? "Unknown repository error"
                    )
            ),
            ApplicationError unknown => (
                HttpStatusCode.InternalServerError,
                ApiError.FromApplicationError(unknown, request)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiError.FromApplicationError(
                    new ApplicationError("UnexpectedError", "An unexpected and unknown error occurred."), request
                )
            )
        };
    }
}