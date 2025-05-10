using Application.Shared.Errors;

namespace Application.UseCases.CheckPulse;

public record CheckPulseUseCaseOutput
{
    public bool IsSuccess { get; init; }
    public Error? Error { get; init; }

    public CheckPulseUseCaseOutput(bool isSuccess, Error? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public CheckPulseUseCaseOutput(Error error)
    {
        IsSuccess = false;
        Error = error;
    }
}