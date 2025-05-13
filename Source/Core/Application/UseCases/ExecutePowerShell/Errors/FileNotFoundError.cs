using Application.Shared.Errors;
using Microsoft.Extensions.Logging;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.UseCases.ExecutePowerShell.Errors;

public class FileNotFoundError : ApplicationError
{
    public ExecutePowerShellInput Input { get; }
    public override LogLevel Severity => LogLevel.Warning;

    public FileNotFoundError(ExecutePowerShellInput input) : base(
        nameof(FileNotFoundError),
        $"PowerShell file -{input.ScriptPath}- could not be found."
    )
    {
        Input = input;
    }
}