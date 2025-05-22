using Application.UseCases.CreateJobSchedule.Abstractions;
using OpenResult;

namespace Application.UseCases.CreateJobSchedule.Infrastructure;

public class CreateJobScheduleInMemoryRepository : ICreateJobScheduleRepository
{
    private List<JobSchedule> Schedules = [];

    public Task<Result<int>> SaveNewSchedule(CreateJobScheduleInput input, CancellationToken cancellationToken = default)
    {
        if (Schedules.Count == 0)
        {
            Schedules.Add(
                new JobSchedule(
                    1,
                    input.Schedule.ToString(),
                    input.JobId,
                    input.Parameters
            ));
            return Task.FromResult(Result.Success(1));
        }
        else
        {
            var last = Schedules.Last();
            var newSchedule = new JobSchedule(
                    last.Id + 1,
                    input.Schedule.ToString(),
                    input.JobId,
                    input.Parameters
            );
            Schedules.Add(newSchedule);
            return Task.FromResult(Result.Success(newSchedule.Id));
        }
    }
}

public record JobSchedule(int Id, string Schedule, Guid JobId, string? Parameters)
{
}