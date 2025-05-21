using Application.Shared;
using Application.Shared.Errors;
using Application.UseCases.CreateJobSchedule;
using Application.UseCases.CreateJobSchedule.Abstractions;
using Application.UseCases.CreateJobSchedule.Errors;
using FluentResults;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using System.Net;
using WebAPI.Minimal.UseCases.CreateJobSchedule;

namespace WebAPI.Minimal.UnitTests.UseCases.CreateJobSchedule;

public class CreateJobScheduleEndpointTests
{
    private readonly ICreateJobScheduleUseCase _useCase;
    private readonly ILogger<CreateJobScheduleEndpoint> _logger;

    public CreateJobScheduleEndpointTests()
    {
        _useCase = Substitute.For<ICreateJobScheduleUseCase>();
        _logger = Substitute.For<ILogger<CreateJobScheduleEndpoint>>();
    }

    [Theory]
    [MemberData(nameof(UseCaseResults))]
    public async Task ShouldReturnJsonResponse_WithMappedResponseAndStatus(Result<CreateJobScheduleOutput> result)
    {
        var request = new CreateJobScheduleRequest(
            JobId: JobTypeRegistry.ExecutePowerShellType.Id,
            Schedule: new RequestSchedule(RequestSchedule.ScheduleType.OneTime, DateTimeOffset.UtcNow.AddMinutes(5).ToString("O")),
            Parameters: "{\"foo\":42}"
        );
        _useCase.Run(Arg.Any<CreateJobScheduleInput>(), Arg.Any<CancellationToken>()).Returns(result);

        var endpointResult = await CreateJobScheduleEndpoint.Execute(_useCase, _logger, request, CancellationToken.None);

        var expectedResponse = new CreateJobScheduleResponse(result, request);
        var json = endpointResult.ShouldBeOfType<JsonHttpResult<CreateJobScheduleResponse>>();
        json.StatusCode.ShouldBe(expectedResponse.StatusCode);
        json.Value.ShouldBeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ShouldReturnJsonResponse_WhenUnknownErrorTypeIsReturned()
    {
        var request = new CreateJobScheduleRequest(
            JobId: Guid.NewGuid(),
            Schedule: new RequestSchedule(RequestSchedule.ScheduleType.OneTime, DateTimeOffset.UtcNow.AddMinutes(5).ToString("O")),
            Parameters: "{\"foo\":42}"
        );

        var unknownErrorResult = Result.Fail<CreateJobScheduleOutput>(
            new Error("Unknown error")
        );
        _useCase.Run(Arg.Any<CreateJobScheduleInput>(), Arg.Any<CancellationToken>())
            .Returns(unknownErrorResult);

        var endpointResult = await CreateJobScheduleEndpoint.Execute(_useCase, _logger, request, CancellationToken.None);

        var json = endpointResult.ShouldBeOfType<JsonHttpResult<CreateJobScheduleResponse>>();
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
            Result.Ok(new CreateJobScheduleOutput(123))
        };
        yield return new object[]
        {
            Result.Fail<CreateJobScheduleOutput>(
                new ValidationError<CreateJobScheduleInput>(
                    new CreateJobScheduleInput(new OneTimeSchedule(DateTimeOffset.MinValue), Guid.NewGuid(), null),
                    new ValidationResult([new ValidationFailure("ScheduledAt", "ScheduledAt must be in the future")])
                )
            )
        };
        yield return new object[]
        {
            Result.Fail<CreateJobScheduleOutput>(
                new JobDoesNotExistError(Guid.NewGuid())
            )
        };
        yield return new object[]
        {
            Result.Fail<CreateJobScheduleOutput>(
                new FailedToSaveScheduleError(Result.Fail<int>("Database unavailable"))
            )
        };
        yield return new object[]
        {
            Result.Fail<CreateJobScheduleOutput>(
                new ApplicationError("SomeError", "Some application-level error")
            )
        };
    }
}