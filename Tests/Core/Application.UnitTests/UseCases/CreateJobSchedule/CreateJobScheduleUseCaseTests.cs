using Application.UseCases.CreateJobSchedule;
using Application.UseCases.CreateJobSchedule.Abstractions;
using NSubstitute;

namespace Application.UnitTests.UseCases.CreateJobSchedule;

public class CreateJobScheduleUseCaseTests
{
    private readonly ICreateJobScheduleRepository _repository;
    private readonly CreateJobScheduleUseCase _useCase;

    public CreateJobScheduleUseCaseTests()
    {
        _repository = Substitute.For<ICreateJobScheduleRepository>();
        _useCase = new(_repository);
    }

    [Fact]
    public async Task ShouldCreateJobSchedule()
    {
        //_repository.SaveNewSchedule();
    }
}