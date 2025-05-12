using Application.Shared.Errors;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.UseCases.ExecutePowerShell.Errors;

public class FileIsNotPowerShellError : ApplicationError
{
    public ExecutePowerShellInput Input { get; }

    public FileIsNotPowerShellError(ExecutePowerShellInput input) : base(
            nameof(FileIsNotPowerShellError),
            $"File {input.ScriptPath} is not a PowerShell file."
        )
    {
        Input = input;
    }
}