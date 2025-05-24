using Application.SchedulingUseCases.ScheduleJob.Abstractions;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Minimal.UseCases.ScheduleJob;

namespace WebAPI.Minimal.SchedulingUseCases.ScheduleJob;

public class ScheduleJobEndpoint
{
    public static async Task<IResult> Execute(
        [FromServices] IScheduleJobUseCase useCase,
        [FromServices] ILogger<ScheduleJobEndpoint> logger,
        [FromBody] ScheduleJobRequest request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Received a request to schedule a job: JobId={JobId}, ScheduleType={ScheduleType}",
            request.JobId, request.Schedule?.Type
        );

        // If Input is Invalid then it Defaults, and then it SHOULD be rejected by the Use Case.
        var input = request.ToInputOrDefault();
        var result = await useCase.Run(input, cancellationToken);

        var response = new ScheduleJobResponse(result, request);

        if (response.IsSuccess)
            logger.LogInformation(
                "Job schedule created successfully: JobScheduleId={JobScheduleId}",
                response.Data!.JobScheduleId
            );
        else
            logger.Log(
                response.Error?.Severity ?? LogLevel.Error,
                response.Error?.Exception,
                "Failed to create job schedule. Error: {Error}",
                response.Error is null ? "ApiError object was null." : response.Error.ToString()
            );

        logger.LogInformation(
            "Returning response with HTTP status code: {StatusCode} ({HttpStatus})",
            response.StatusCode, response.HttpStatusCode
        );
        return Results.Json(data: response, statusCode: response.StatusCode);
    }
}