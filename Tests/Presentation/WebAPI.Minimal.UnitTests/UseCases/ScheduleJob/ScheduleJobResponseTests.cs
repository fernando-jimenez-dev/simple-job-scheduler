using Application.Shared.Errors;
using Application.UseCases.ScheduleJob;
using Application.UseCases.ScheduleJob.Errors;
using FluentValidation.Results;
using OpenResult;
using Shouldly;
using System.Net;
using WebAPI.Minimal.UseCases.ScheduleJob;

namespace WebAPI.Minimal.UnitTests.UseCases.ScheduleJob;

public class ScheduleJobResponseTests
{
    private readonly ScheduleJobRequest _request = new(
        JobId: Guid.NewGuid(),
        Schedule: new RequestSchedule(RequestSchedule.ScheduleType.OneTime, DateTimeOffset.UtcNow.AddMinutes(5).ToString("O")),
        Parameters: "{\"foo\": 42}"
    );

    [Fact]
    public void ShouldMapTo201Created_OnSuccess()
    {
        // Arrange
        var useCaseOutput = new ScheduleJobOutput(JobScheduleId: 123);
        var result = Result.Success(useCaseOutput);

        // Act
        var response = new ScheduleJobResponse(result, _request);

        // Assert
        response.IsSuccess.ShouldBeTrue();
        response.Data.ShouldNotBeNull().JobScheduleId.ShouldBe(123);
        response.HttpStatusCode.ShouldBe(ScheduleJobResponse.SuccessCode);
        response.Error.ShouldBeNull();
    }

    [Fact]
    public void ShouldMapTo400BadRequest_OnValidationError()
    {
        // Arrange
        var input = _request.ToInputOrDefault();
        var validationError = new ValidationError<ScheduleJobInput>(input, new ValidationResult([
            new ValidationFailure("ScheduledAt", "ScheduledAt must be in the future")
        ]));
        var result = Result<ScheduleJobOutput>.Failure(validationError);

        // Act
        var response = new ScheduleJobResponse(result, _request);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Data.ShouldBeNull();
        response.HttpStatusCode.ShouldBe(ScheduleJobResponse.ValidationErrorCode);
        response.Error.ShouldNotBeNull();
        response.Error.Type.ShouldBe(ValidationError<ScheduleJobInput>.Name);
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
        var result = Result<ScheduleJobOutput>.Failure(error);

        // Act
        var response = new ScheduleJobResponse(result, _request);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Data.ShouldBeNull();
        response.HttpStatusCode.ShouldBe(ScheduleJobResponse.JobDoesNotExistErrorCode);
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
        var saveResult = Result<int>.Failure(new Error("Database unavailable"));
        var error = new FailedToSaveScheduleError(saveResult);
        var result = Result<ScheduleJobOutput>.Failure(error);

        // Act
        var response = new ScheduleJobResponse(result, _request);

        // Assert
        response.IsSuccess.ShouldBeFalse();
        response.Data.ShouldBeNull();
        response.HttpStatusCode.ShouldBe(ScheduleJobResponse.FailedToSaveScheduleErrorCode);
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
        var result = Result<ScheduleJobOutput>.Failure(error);

        // Act
        var response = new ScheduleJobResponse(result, _request);

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
        var result = Result<ScheduleJobOutput>.Failure(unknownError);

        // Act
        var response = new ScheduleJobResponse(result, _request);

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