using FluentResults;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.UseCases.ExecutePowerShell.Errors;

public class FailureExitCodeError : Error
{
    public readonly ExecutePowerShellInput Input;
    public readonly int ExitCode;
    public readonly string? StandardOutput;
    public readonly string? StandardError;

    public FailureExitCodeError
    (
        ExecutePowerShellInput input,
        int exitCode,
        string? standardOutput = null,
        string? standardError = null
    )
    : base($"PowerShell execution returned exit code '{exitCode}'.")
    {
        Input = input;
        ExitCode = exitCode;
        StandardOutput = standardOutput;
        StandardError = standardError;

        Metadata.Add("Guid", Guid.NewGuid());
        Metadata.Add("Type", nameof(FailureExitCodeError));
    }
}