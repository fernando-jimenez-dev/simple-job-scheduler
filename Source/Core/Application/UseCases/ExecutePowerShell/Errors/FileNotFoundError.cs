using FluentResults;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.UseCases.ExecutePowerShell.Errors;

public class FileNotFoundError : Error
{
    public readonly ExecutePowerShellInput Input;

    public FileNotFoundError(ExecutePowerShellInput input)
    : base($"PowerShell file {input.ScriptPath} could not be found.")
    {
        Input = input;

        Metadata.Add("Guid", Guid.NewGuid());
        Metadata.Add("Type", nameof(FileNotFoundError));
    }
}