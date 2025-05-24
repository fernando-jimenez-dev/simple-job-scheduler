using Application.Shared.Abstractions.UseCase;
using Application.SchedulingUseCases.ScheduleJob;

namespace Application.SchedulingUseCases.ScheduleJob.Abstractions;

public interface IScheduleJobUseCase : IUseCase<ScheduleJobInput, ScheduleJobOutput>
{
}