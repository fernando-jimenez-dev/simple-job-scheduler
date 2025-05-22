using OpenResult;

namespace Application.UseCases.CreateJobSchedule.Abstractions;

public interface ICreateJobScheduleRepository
{
    /// <returns>Id of the saved schedule</returns>
    Task<Result<int>> SaveNewSchedule(CreateJobScheduleInput input, CancellationToken cancellationToken = default);
}

public record SaveNewScheduleInput(DateTimeOffset Schedule, int JobId, string Parameters)
{
}

// Potential errors
// - JobId does not exist
// - Job does not accept parameters