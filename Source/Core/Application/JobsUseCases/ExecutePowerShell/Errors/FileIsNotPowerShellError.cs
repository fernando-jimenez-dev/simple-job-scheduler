using Application.Shared.Errors;
using Microsoft.Extensions.Logging;
using static Application.JobsUseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.JobsUseCases.ExecutePowerShell.Errors;

public record FileIsNotPowerShellError : ApplicationError
{
    public ExecutePowerShellInput Input { get; }
    public override LogLevel Severity => LogLevel.Warning;

    public FileIsNotPowerShellError(ExecutePowerShellInput input) : base(
            nameof(FileIsNotPowerShellError),
            $"File -{input.ScriptPath}- is not a PowerShell file."
        )
    {
        Input = input;
    }
}