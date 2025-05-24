using Application.Shared.Errors;
using Microsoft.Extensions.Logging;
using static Application.JobsUseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.JobsUseCases.ExecutePowerShell.Errors;

public record FailureExitCodeError : ApplicationError
{
    public ExecutePowerShellInput Input { get; }
    public int ExitCode { get; }
    public string? StandardOutput { get; }
    public string? StandardError { get; }

    public override LogLevel Severity => LogLevel.Warning;

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