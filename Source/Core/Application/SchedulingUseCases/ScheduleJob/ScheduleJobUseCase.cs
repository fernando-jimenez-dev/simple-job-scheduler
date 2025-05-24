using Application.Shared;
using Application.Shared.Errors;
using Application.SchedulingUseCases.ScheduleJob.Abstractions;
using Application.SchedulingUseCases.ScheduleJob.Errors;
using OpenResult;

namespace Application.SchedulingUseCases.ScheduleJob;

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
public class ScheduleJobUseCase : IScheduleJobUseCase
{
    private readonly IScheduleJobRepository _repository;

    public ScheduleJobUseCase(IScheduleJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ScheduleJobOutput>> Run(ScheduleJobInput input, CancellationToken cancellationToken = default)
    {
        // 2.1 Validate input
        var validationResult = input.Validate();
        if (!validationResult.IsValid)
            return Fail(ValidationError.For(input, validationResult));

        // 2.2 Job Id is not registered.
        if (!JobTypeRegistry.JobExists(input.JobId))
            return Fail(JobDoesNotExistError.For(input.JobId));

        // 3. Register schedule
        var saveNewScheduleResult = await _repository.SaveNewSchedule(input, cancellationToken);
        if (!saveNewScheduleResult.IsSuccess)
            return Fail(FailedToSaveScheduleError.For(saveNewScheduleResult));

        var newScheduleId = saveNewScheduleResult.Value;
        return Result.Success(new ScheduleJobOutput(newScheduleId));
    }

    private static Result<ScheduleJobOutput> Fail(ApplicationError error)
        => Result<ScheduleJobOutput>.Failure(error);
}