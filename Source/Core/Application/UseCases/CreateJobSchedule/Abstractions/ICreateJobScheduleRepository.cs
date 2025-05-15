using FluentResults;

namespace Application.UseCases.CreateJobSchedule.Abstractions;

public interface ICreateJobScheduleRepository
{
    /// <returns>Id of the saved schedule</returns>
    Task<Result<int>> SaveNewSchedule(SaveNewScheduleInput input, CancellationToken cancellationToken = default);

    Task<Result<bool>> JobIdExists(int JobId, CancellationToken cancellationToken = default);

    //Task<Result<bool>> JobAcceptsParameters(int)
}

public record SaveNewScheduleInput(DateTimeOffset Schedule, int JobId, string Parameters)
{
}

// Potential errors
// - JobId does not exist
// - Job does not accept parameters