using FluentResults;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.UseCases.ExecutePowerShell.Errors;

public class FileIsNotPowerShellError : Error
{
    public readonly ExecutePowerShellInput Input;

    public FileIsNotPowerShellError(ExecutePowerShellInput input)
    : base($"File {input.ScriptPath} is not a PowerShell file.")
    {
        Input = input;

        Metadata.Add("Guid", Guid.NewGuid());
        Metadata.Add("Type", nameof(FileIsNotPowerShellError));
    }
}