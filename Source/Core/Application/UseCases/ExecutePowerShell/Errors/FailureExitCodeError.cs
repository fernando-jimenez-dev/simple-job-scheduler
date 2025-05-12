using Application.Shared.Errors;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.UseCases.ExecutePowerShell.Errors;

public class FailureExitCodeError : ApplicationError
{
    public ExecutePowerShellInput Input { get; }
    public int ExitCode { get; }
    public string? StandardOutput { get; }
    public string? StandardError { get; }

    public FailureExitCodeError
    (
        ExecutePowerShellInput input,
        int exitCode,
        string? standardOutput = null,
        string? standardError = null
    )
    : base(
        nameof(FailureExitCodeError),
        $"PowerShell execution returned exit code '{exitCode}'."
    )
    {
        Input = input;
        ExitCode = exitCode;
        StandardOutput = standardOutput;
        StandardError = standardError;
    }
}