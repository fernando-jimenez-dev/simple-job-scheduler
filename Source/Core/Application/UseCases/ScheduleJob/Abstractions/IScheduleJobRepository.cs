using OpenResult;

namespace Application.UseCases.ScheduleJob.Abstractions;

public interface IScheduleJobRepository
{
    /// <returns>Id of the saved schedule</returns>
    Task<Result<int>> SaveNewSchedule(ScheduleJobInput input, CancellationToken cancellationToken = default);
}

public record SaveNewScheduleInput(DateTimeOffset Schedule, int JobId, string Parameters)
{
}