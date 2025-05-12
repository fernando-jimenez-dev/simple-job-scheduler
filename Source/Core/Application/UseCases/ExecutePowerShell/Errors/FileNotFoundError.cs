using Application.Shared.Errors;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.UseCases.ExecutePowerShell.Errors;

public class FileNotFoundError : ApplicationError
{
    public ExecutePowerShellInput Input { get; }

    public FileNotFoundError(ExecutePowerShellInput input) : base(
        nameof(FileNotFoundError),
        $"PowerShell file {input.ScriptPath} could not be found."
    )
    {
        Input = input;
    }
}