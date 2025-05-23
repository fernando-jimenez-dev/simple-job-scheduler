using Application.UseCases.ScheduleJob;
using FluentValidation.Results;
using Shouldly;

namespace Application.UnitTests.UseCases.ScheduleJob;

public class ScheduleJobInputTests
{
    [Fact]
    public void ShouldSucceed_WithValidOneTimeSchedule_AndNoParameters()
    {
        // Arrange
        var schedule = new OneTimeSchedule(DateTimeOffset.UtcNow.AddMinutes(10));
        var jobId = Guid.NewGuid();
        var input = new ScheduleJobInput(schedule, jobId);

        // Act
        var result = input.Validate();

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldSucceed_WithValidCronSchedule_AndParameters()
    {
        // Arrange
        var schedule = new CronSchedule("0 12 * * 1-5"); // Noon every weekday
        var jobId = Guid.NewGuid();
        var parameters = "{\"foo\": 42}";
        var input = new ScheduleJobInput(schedule, jobId, parameters);

        // Act
        var result = input.Validate();

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldFail_WhenJobIdIsEmpty()
    {
        // Arrange
        var schedule = new OneTimeSchedule(DateTimeOffset.UtcNow.AddMinutes(10));
        var jobId = Guid.Empty;
        var input = new ScheduleJobInput(schedule, jobId);

        // Act
        var result = input.Validate();

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "JobId");
    }

    [Fact]
    public void ShouldFail_WhenScheduleIsInvalid()
    {
        // Arrange: OneTimeSchedule in the past
        var schedule = new OneTimeSchedule(DateTimeOffset.UtcNow.AddMinutes(-10));
        var jobId = Guid.NewGuid();
        var input = new ScheduleJobInput(schedule, jobId);

        // Act
        var result = input.Validate();

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName.Contains("ScheduledAt"));
    }

    [Fact]
    public void ShouldFail_WhenParametersIsInvalidJson()
    {
        // Arrange
        var schedule = new OneTimeSchedule(DateTimeOffset.UtcNow.AddMinutes(10));
        var jobId = Guid.NewGuid();
        var input = new ScheduleJobInput(schedule, jobId, "{notJson}");

        // Act
        var result = input.Validate();

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Parameters");
    }

    [Fact]
    public void ShouldSucceed_WhenParametersIsNullOrEmpty()
    {
        // Arrange
        var schedule = new OneTimeSchedule(DateTimeOffset.UtcNow.AddMinutes(10));
        var jobId = Guid.NewGuid();

        // Act/Assert
        new ScheduleJobInput(schedule, jobId, null).Validate().IsValid.ShouldBeTrue();
        new ScheduleJobInput(schedule, jobId, "").Validate().IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldFail_WhenScheduleTypeIsUnknown()
    {
        // Arrange
        var schedule = new UnknownSchedule();
        var jobId = Guid.NewGuid();
        var input = new ScheduleJobInput(schedule, jobId);

        // Act
        var result = input.Validate();

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    private record UnknownSchedule : Schedule
    {
        public override ValidationResult Validate() => new ValidationResult(
            [new ValidationFailure("Unknown", "Unknown schedule type")]
        );
    }
}

public class OneTimeScheduleTests
{
    [Fact]
    public void ShouldSucceed_WhenScheduledAtIsInTheFuture()
    {
        var schedule = new OneTimeSchedule(DateTimeOffset.UtcNow.AddMinutes(1));
        var result = schedule.Validate();
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldFail_WhenScheduledAtIsNowOrPast()
    {
        var now = DateTimeOffset.UtcNow;
        var schedule1 = new OneTimeSchedule(now);
        var schedule2 = new OneTimeSchedule(now.AddMinutes(-5));
        schedule1.Validate().IsValid.ShouldBeFalse();
        schedule2.Validate().IsValid.ShouldBeFalse();
    }
}

public class CronScheduleTests
{
    [Theory]
    [InlineData("0 0 * * *")] // Valid 5-field
    [InlineData("0 0 0 * * *")] // Valid 6-field
    [InlineData("*/5 9-17 * * MON-FRI")] // Valid
    public void ShouldSucceed_WhenCronExpressionIsValid(string expr)
    {
        var cron = new CronSchedule(expr);
        var result = cron.Validate();
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("bad cron")]
    [InlineData("0 0 0 0 0 0 0 0")] // Too many fields
    [InlineData("0 0 0 0")] // Too few fields
    [InlineData("60 12 * * *")] // Invalid minute value
    public void ShouldFail_WhenCronExpressionIsInvalid(string expr)
    {
        var cron = new CronSchedule(expr);
        var result = cron.Validate();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "CronExpression");
    }
}