using Application.Shared.Abstractions.UseCase;

namespace Application.UseCases.ScheduleJob.Abstractions;

public interface IScheduleJobUseCase : IUseCase<ScheduleJobInput, ScheduleJobOutput>
{
}