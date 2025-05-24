using Application.SchedulingUseCases.ScheduleJob;
using Application.SchedulingUseCases.ScheduleJob.Abstractions;
using Application.SchedulingUseCases.ScheduleJob.Errors;
using Application.Shared;
using Application.Shared.Errors;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenResult;
using Shouldly;
using System.Net;
using WebAPI.Minimal.SchedulingUseCases.ScheduleJob;

namespace WebAPI.Minimal.UnitTests.SchedulingUseCases.ScheduleJob;

public class ScheduleJobEndpointTests
{
    private readonly IScheduleJobUseCase _useCase;
    private readonly ILogger<ScheduleJobEndpoint> _logger;

    public ScheduleJobEndpointTests()
    {
        _useCase = Substitute.For<IScheduleJobUseCase>();
        _logger = Substitute.For<ILogger<ScheduleJobEndpoint>>();
    }

    [Theory]
    [MemberData(nameof(UseCaseResults))]
    public async Task ShouldReturnJsonResponse_WithMappedResponseAndStatus(Result<ScheduleJobOutput> result)
    {
        var request = new ScheduleJobRequest(
            JobId: JobTypeRegistry.ExecutePowerShellType.Id,
            Schedule: new RequestSchedule(RequestSchedule.ScheduleType.OneTime, DateTimeOffset.UtcNow.AddMinutes(5).ToString("O")),
            Parameters: "{\"foo\":42}"
        );
        _useCase.Run(Arg.Any<ScheduleJobInput>(), Arg.Any<CancellationToken>()).Returns(result);

        var endpointResult = await ScheduleJobEndpoint.Execute(_useCase, _logger, request, CancellationToken.None);

        var expectedResponse = new ScheduleJobResponse(result, request);
        var json = endpointResult.ShouldBeOfType<JsonHttpResult<ScheduleJobResponse>>();
        json.StatusCode.ShouldBe(expectedResponse.StatusCode);
        json.Value.ShouldBeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ShouldReturnJsonResponse_WhenUnknownErrorTypeIsReturned()
    {
        var request = new ScheduleJobRequest(
            JobId: Guid.NewGuid(),
            Schedule: new RequestSchedule(RequestSchedule.ScheduleType.OneTime, DateTimeOffset.UtcNow.AddMinutes(5).ToString("O")),
            Parameters: "{\"foo\":42}"
        );

        var unknownErrorResult = Result<ScheduleJobOutput>.Failure(
            new Error("Unknown error")
        );
        _useCase.Run(Arg.Any<ScheduleJobInput>(), Arg.Any<CancellationToken>())
            .Returns(unknownErrorResult);

        var endpointResult = await ScheduleJobEndpoint.Execute(_useCase, _logger, request, CancellationToken.None);

        var json = endpointResult.ShouldBeOfType<JsonHttpResult<ScheduleJobResponse>>();
        json.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        json.Value.ShouldNotBeNull();
        json.Value.Error.ShouldNotBeNull();
        json.Value.Error.Id.ToString().ShouldNotBeEmpty();
        json.Value.Error.Type.ShouldBe("UnexpectedError");
        json.Value.Error.Message.ShouldBe("An unexpected and unknown error occurred.");
    }

    public static IEnumerable<object[]> UseCaseResults()
    {
        yield return new object[]
        {
            Result.Success(new ScheduleJobOutput(123))
        };
        yield return new object[]
        {
            Result<ScheduleJobOutput>.Failure(
                new ValidationError<ScheduleJobInput>(
                    new ScheduleJobInput(new OneTimeSchedule(DateTimeOffset.MinValue), Guid.NewGuid(), null),
                    new ValidationResult([new ValidationFailure("ScheduledAt", "ScheduledAt must be in the future")])
                )
            )
        };
        yield return new object[]
        {
            Result<ScheduleJobOutput>.Failure(
                new JobDoesNotExistError(Guid.NewGuid())
            )
        };
        yield return new object[]
        {
            Result <ScheduleJobOutput>.Failure(
                new FailedToSaveScheduleError(Result<int>.Failure(new Error("Database unavailable")))
            )
        };
        yield return new object[]
        {
            Result <ScheduleJobOutput>.Failure(
                new ApplicationError("SomeError", "Some application-level error")
            )
        };
    }
}