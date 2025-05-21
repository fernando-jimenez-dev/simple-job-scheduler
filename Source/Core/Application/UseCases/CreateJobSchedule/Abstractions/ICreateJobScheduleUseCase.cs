using Application.Shared.Abstractions.UseCase;

namespace Application.UseCases.CreateJobSchedule.Abstractions;

public interface ICreateJobScheduleUseCase : IUseCase<CreateJobScheduleInput, CreateJobScheduleOutput>
{
}