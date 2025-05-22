using Application.Shared;
using Application.Shared.Errors;
using Application.UseCases.CreateJobSchedule;
using Application.UseCases.CreateJobSchedule.Abstractions;
using Application.UseCases.CreateJobSchedule.Errors;
using NSubstitute;
using OpenResult;
using Shouldly;

namespace Application.UnitTests.UseCases.CreateJobSchedule;

public class CreateJobScheduleUseCaseTests
{
    private readonly ICreateJobScheduleRepository _repository;
    private readonly CreateJobScheduleUseCase _useCase;
    private readonly Guid _registeredJobTypeId = JobTypeRegistry.ExecutePowerShellType.Id;

    public CreateJobScheduleUseCaseTests()
    {
        _repository = Substitute.For<ICreateJobScheduleRepository>();
        _useCase = new CreateJobScheduleUseCase(_repository);
    }

    [Fact]
    public async Task ShouldSucceed_WhenInputIsValid_AndRepositorySucceeds()
    {
        // Arrange
        var schedule = new OneTimeSchedule(DateTimeOffset.UtcNow.AddMinutes(5));
        var input = new CreateJobScheduleInput(schedule, _registeredJobTypeId, "{\"foo\":42}");

        const int createdScheduleId = 777;
        _repository
            .SaveNewSchedule(input, Arg.Any<CancellationToken>())
            .Returns(Result.Success(createdScheduleId));

        // Act
        var result = await _useCase.Run(input);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var output = result.Value.ShouldNotBeNull();
        output.JobScheduleId.ShouldBe(createdScheduleId);
    }

    [Fact]
    public async Task ShouldFail_WithValidationError_WhenScheduleIsInvalid()
    {
        // Arrange: past time
        var schedule = new OneTimeSchedule(DateTimeOffset.UtcNow.AddMinutes(-10));
        var input = new CreateJobScheduleInput(schedule, _registeredJobTypeId);

        // Act
        var result = await _useCase.Run(input);

        // Assert
        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldNotBeNull().Root;
        var validationError = error.ShouldBeOfType<ValidationError<CreateJobScheduleInput>>();
        validationError.Value.ShouldBe(input);
        validationError.Issues.ShouldContain(x => x.Contains("ScheduledAt must be in the future"));
        validationError.Id.ToString().ShouldNotBeNullOrEmpty();
        validationError.Type.ShouldBe("ValidationError<CreateJobScheduleInput>");
        await _repository.DidNotReceive().SaveNewSchedule(Arg.Any<CreateJobScheduleInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldFail_WithJobDoesNotExistError_WhenJobIdIsUnknown()
    {
        // Arrange: use unregistered job id
        var unknownJobId = Guid.NewGuid();
        var schedule = new OneTimeSchedule(DateTimeOffset.UtcNow.AddMinutes(10));
        var input = new CreateJobScheduleInput(schedule, unknownJobId);

        // Act
        var result = await _useCase.Run(input);

        // Assert
        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldNotBeNull().Root;
        var jobDoesNotExistError = error.ShouldBeOfType<JobDoesNotExistError>();
        jobDoesNotExistError.JobId.ShouldBe(unknownJobId);
        jobDoesNotExistError.Id.ToString().ShouldNotBeNullOrEmpty();
        jobDoesNotExistError.Type.ShouldBe(nameof(JobDoesNotExistError));
        await _repository.DidNotReceive().SaveNewSchedule(Arg.Any<CreateJobScheduleInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldFail_WithFailedToSaveScheduleError_WhenRepositoryFails()
    {
        // Arrange
        var schedule = new OneTimeSchedule(DateTimeOffset.UtcNow.AddMinutes(10));
        var input = new CreateJobScheduleInput(schedule, _registeredJobTypeId);

        _repository
            .SaveNewSchedule(input, Arg.Any<CancellationToken>())
            .Returns(Result<int>.Failure(new Error("Database is down")));

        // Act
        var result = await _useCase.Run(input);

        // Assert
        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldNotBeNull().Root;
        var failedToSaveError = error.ShouldBeOfType<FailedToSaveScheduleError>();
        failedToSaveError.SaveScheduleResult.IsSuccess.ShouldBeFalse();
        failedToSaveError.SaveScheduleResult.Error.ShouldNotBeNull();
        failedToSaveError.Id.ToString().ShouldNotBeNullOrEmpty();
        failedToSaveError.Type.ShouldBe(nameof(FailedToSaveScheduleError));
    }

    [Fact]
    public async Task ShouldFail_WithValidationError_WhenParametersIsInvalidJson()
    {
        // Arrange: Invalid JSON
        var schedule = new OneTimeSchedule(DateTimeOffset.UtcNow.AddMinutes(10));
        var input = new CreateJobScheduleInput(schedule, _registeredJobTypeId, "{not_json}");

        // Act
        var result = await _useCase.Run(input);

        // Assert
        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldNotBeNull().Root;
        var validationError = error.ShouldBeOfType<ValidationError<CreateJobScheduleInput>>();
        validationError.Value.ShouldBe(input);
        validationError.Issues.ShouldContain(x => x.Contains("Parameters must be a valid JSON string"));
        validationError.Id.ToString().ShouldNotBeNullOrEmpty();
        validationError.Type.ShouldBe("ValidationError<CreateJobScheduleInput>");
        await _repository.DidNotReceive().SaveNewSchedule(Arg.Any<CreateJobScheduleInput>(), Arg.Any<CancellationToken>());
    }
}