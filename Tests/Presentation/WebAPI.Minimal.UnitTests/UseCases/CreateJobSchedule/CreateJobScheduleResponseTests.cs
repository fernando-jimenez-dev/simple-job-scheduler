using Application.Shared.Errors;
using Application.UseCases.CreateJobSchedule;
using Application.UseCases.CreateJobSchedule.Errors;
using FluentResults;
using FluentValidation.Results;
using Shouldly;
using System.Net;
using WebAPI.Minimal.UseCases.CreateJobSchedule;

namespace WebAPI.Minimal.UnitTests.UseCases.CreateJobSchedule;

public class CreateJobScheduleResponseTests
{
    private readonly CreateJobScheduleRequest _request = new(
        JobId: Guid.NewGuid(),
        Schedule: new RequestSchedule(RequestSchedule.ScheduleType.OneTime, DateTimeOffset.UtcNow.AddMinutes(5).ToString("O")),
        Parameters: "{\"foo\": 42}"
    );

    [Fact]
    public void ShouldMapTo201Created_OnSuccess()
    {
        // Arrange
        var useCaseOutput = new CreateJobScheduleOutput(JobScheduleId: 123);
        var result = Result.Ok(useCaseOutput);

        // Act
        var response = new CreateJobScheduleResponse(result, _request);

        // Assert
        response.IsSuccess.ShouldBeTrue();
        response.Data.ShouldNotBeNull().JobScheduleId.ShouldBe(123);
        response.HttpStatusCode.ShouldBe(CreateJobScheduleResponse.SuccessCode);
        response.Error.ShouldBeNull();
    }

    [Fact]
    public void ShouldMapTo400BadRequest_OnValidationError()
    {
        // Arrange
        var input = _request.ToInputOrDefault();
        var validationError = new ValidationError<CreateJobScheduleInput>(input, new ValidationResult([
            new ValidationFailure("ScheduledAt", "ScheduledAt must be in the future")
        ]));
        var result = Result.Fail<CreateJobScheduleOutput>(validationError);

        // Act
        var response = new CreateJobScheduleResponse(result, _request);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Data.ShouldBeNull();
        response.HttpStatusCode.ShouldBe(CreateJobScheduleResponse.ValidationErrorCode);
        response.Error.ShouldNotBeNull();
        response.Error.Type.ShouldBe(ValidationError<CreateJobScheduleInput>.Name);
        response.Error.Details.ShouldContainKey("validationIssues");
        var issues = response.Error.Details["validationIssues"].ShouldBeOfType<List<string>>();
        var issue = issues.ShouldNotBeNull().First();
        issue.ShouldContain("ScheduledAt must be in the future");
    }

    [Fact]
    public void ShouldMapTo404NotFound_OnJobDoesNotExistError()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var error = new JobDoesNotExistError(jobId);
        var result = Result.Fail<CreateJobScheduleOutput>(error);

        // Act
        var response = new CreateJobScheduleResponse(result, _request);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Data.ShouldBeNull();
        response.HttpStatusCode.ShouldBe(CreateJobScheduleResponse.JobDoesNotExistErrorCode);
        response.Error.ShouldNotBeNull();
        response.Error.Type.ShouldBe(JobDoesNotExistError.Name);
        response.Error.Details.ShouldContainKey("jobId");
        var id = response.Error.Details["jobId"].ShouldBeOfType<Guid>();
        id.ShouldBe(jobId);
    }

    [Fact]
    public void ShouldMapTo500InternalServerError_OnFailedToSaveScheduleError()
    {
        // Arrange
        var saveResult = Result.Fail<int>("Database unavailable");
        var error = new FailedToSaveScheduleError(saveResult);
        var result = Result.Fail<CreateJobScheduleOutput>(error);

        // Act
        var response = new CreateJobScheduleResponse(result, _request);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Data.ShouldBeNull();
        response.HttpStatusCode.ShouldBe(CreateJobScheduleResponse.FailedToSaveScheduleErrorCode);
        response.Error.ShouldNotBeNull();
        response.Error.Type.ShouldBe(FailedToSaveScheduleError.Name);
        response.Error.Details.ShouldContainKey("repositoryError");
        var msg = response.Error.Details["repositoryError"].ShouldBeOfType<string>();
        msg.ShouldBe("Database unavailable");
    }

    [Fact]
    public void ShouldMapTo500InternalServerError_OnApplicationError()
    {
        // Arrange
        var error = new ApplicationError("SomeError", "Some application-level error");
        var result = Result.Fail<CreateJobScheduleOutput>(error);

        // Act
        var response = new CreateJobScheduleResponse(result, _request);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Data.ShouldBeNull();
        response.HttpStatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Error.ShouldNotBeNull();
        response.Error.Type.ShouldBe("SomeError");
        response.Error.Message.ShouldBe("Some application-level error");
    }

    [Fact]
    public void ShouldMapTo500InternalServerError_OnUnknownErrorType()
    {
        // Arrange
        var unknownError = new Error("Unknown error");
        var result = Result.Fail<CreateJobScheduleOutput>(unknownError);

        // Act
        var response = new CreateJobScheduleResponse(result, _request);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Data.ShouldBeNull();
        response.HttpStatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Error.ShouldNotBeNull();
        response.Error.Type.ShouldBe("UnexpectedError");
        response.Error.Message.ShouldBe("An unexpected and unknown error occurred.");
    }
}

public class RequestScheduleTests
{
    [Fact]
    public void ToDomainOrDefault_ShouldReturnOneTimeSchedule_WhenTypeIsOneTimeAndValueIsDate()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var schedule = new RequestSchedule(RequestSchedule.ScheduleType.OneTime, now.ToString("O"));

        // Act
        var result = schedule.ToDomainOrDefault();

        // Assert
        var oneTimeSchedule = result.ShouldBeOfType<OneTimeSchedule>();
        oneTimeSchedule.ScheduledAt.ShouldBe(now);
    }

    [Fact]
    public void ToDomainOrDefault_ShouldReturnCronSchedule_WhenTypeIsCronSchedule()
    {
        // Arrange
        var cron = "* * * * *";
        var schedule = new RequestSchedule(RequestSchedule.ScheduleType.CronSchedule, cron);

        // Act
        var result = schedule.ToDomainOrDefault();

        // Assert
        var cronSchedule = result.ShouldBeOfType<CronSchedule>();
        cronSchedule.CronExpression.ShouldBe(cron);
    }

    [Fact]
    public void ToDomainOrDefault_ShouldReturnDefaultInvalid_WhenTypeIsOneTimeAndValueIsInvalidDate()
    {
        // Arrange
        var schedule = new RequestSchedule(RequestSchedule.ScheduleType.OneTime, "not-a-date");

        // Act
        var result = schedule.ToDomainOrDefault();

        // Assert
        var invalidOneTimeSchedule = result.ShouldBeOfType<OneTimeSchedule>();
        invalidOneTimeSchedule.ScheduledAt.ShouldBe(DateTimeOffset.MinValue);
    }
}