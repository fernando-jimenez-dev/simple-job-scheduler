using Application.Shared.Errors;
using Application.UseCases.CreateJobSchedule.Abstractions;
using FluentResults;

namespace Application.UseCases.CreateJobSchedule;

/// <summary>
/// 1. Schedule a One-Time Job Execution
/// Purpose:
/// Allow the system to accept a request to run a specific job once at a specified future time.
///
/// Steps:
/// 1. Receive a scheduling request that includes:
///   . The exact future time for execution.
///   . The type of job to be executed.
///   . The parameters required by the job.
/// 2. Validate:
///   . That the scheduled time is valid (future, not past).
///   . That the job type is recognized or deferrable to runtime validation.
/// 3. Register:
///   . Store a new entry in the system representing this scheduled job.
///   . Mark it as pending and track its relevant metadata.
/// 4. Acknowledge the request (e.g., with a tracking ID or status).
/// </summary>
public class CreateJobScheduleUseCase : ICreateJobScheduleUseCase
{
    private readonly ICreateJobScheduleRepository _repository;

    public CreateJobScheduleUseCase(ICreateJobScheduleRepository repository)
    {
        _repository = repository;
    }

    public Task<Result<CreateJobScheduleOutput>> Run(CreateJobScheduleInput input, CancellationToken cancellationToken = default)
    {
        // 2. Validate input
        var validationResult = input.Validate();
        if (!validationResult.IsValid) Fail(ValidationError.For(input, validationResult));

        throw new NotImplementedException();
    }

    private static Result<CreateJobScheduleOutput> Fail(ApplicationError error)
        => Result.Fail<CreateJobScheduleOutput>(error);
}