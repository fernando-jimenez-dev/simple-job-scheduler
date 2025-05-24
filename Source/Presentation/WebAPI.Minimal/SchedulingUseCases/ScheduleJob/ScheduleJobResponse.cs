using Application.Shared.Errors;
using Application.SchedulingUseCases.ScheduleJob;
using Application.SchedulingUseCases.ScheduleJob.Errors;
using OpenResult;
using System.Net;
using WebAPI.Minimal.Shared;

namespace WebAPI.Minimal.SchedulingUseCases.ScheduleJob;

public record ScheduleJobResponse
{
    public bool IsSuccess { get; init; }
    public ScheduleJobOutput? Data { get; init; }
    public ApiError? Error { get; init; }
    public HttpStatusCode HttpStatusCode { get; init; }
    public int StatusCode => (int)HttpStatusCode;

    public const HttpStatusCode SuccessCode = HttpStatusCode.Created;
    public const HttpStatusCode ValidationErrorCode = HttpStatusCode.BadRequest;
    public const HttpStatusCode JobDoesNotExistErrorCode = HttpStatusCode.NotFound;
    public const HttpStatusCode FailedToSaveScheduleErrorCode = HttpStatusCode.InternalServerError;

    public ScheduleJobResponse(Result<ScheduleJobOutput> useCaseResult, ScheduleJobRequest request)
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

        var useCaseError = useCaseResult.Error!.Root;
        (HttpStatusCode, Error) = MapErrorToHttp(useCaseError, request);
    }

    private static (HttpStatusCode, ApiError) MapErrorToHttp(Error useCaseError, ScheduleJobRequest request)
    {
        return useCaseError switch
        {
            ValidationError<ScheduleJobInput> error => (
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
                        error.SaveScheduleResult.Error?.Root.Message ?? "Unknown repository error"
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