using Application.UseCases.ExecutePowerShell.Abstractions;
using Application.UseCases.ExecutePowerShell.Errors;
using FluentResults;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.UseCases.ExecutePowerShell;

/// <summary>
/// This use case allows the scheduler to execute a local PowerShell script
/// given a fully qualified file path. It performs a direct invocation of the
/// script using the system's PowerShell runtime, waits for execution to complete,
/// and captures the result. The output includes whether the execution was successful,
/// the script's exit code, and any text written to standard output or standard error.
/// This behavior is synchronous and immediate—no scheduling, retries,
/// or side effects are involved. The purpose is to run real work on demand
/// and return structured feedback that a developer or upstream orchestrator
/// can reason about deterministically.
/// </summary>
public class ExecutePowerShellUseCase : IExecutePowerShellUseCase
{
    private readonly IScriptFileVerifier _scriptFileVerifier;
    private readonly IPowerShellExecutor _powerShellExecutor;

    public ExecutePowerShellUseCase(IPowerShellExecutor powerShellExecutor, IScriptFileVerifier scriptFileVerifier)
    {
        _powerShellExecutor = powerShellExecutor;
        _scriptFileVerifier = scriptFileVerifier;
    }

    public async Task<Result<ExecutePowerShellOutput>> Run(ExecutePowerShellInput input, CancellationToken cancellationToken = default)
    {
        // 0. Validate Input
        var validationResult = input.Validate();
        if (!validationResult.IsValid)
            return Result.Fail<ExecutePowerShellOutput>(new InvalidInputError(input, validationResult));

        // 1. Validate Path
        if (!_scriptFileVerifier.Exists(input.ScriptPath))
            return Result.Fail<ExecutePowerShellOutput>(new FileNotFoundError(input));

        // 2. Validate Extension
        if (!_scriptFileVerifier.IsPowerShell(input.ScriptPath))
            return Result.Fail<ExecutePowerShellOutput>(new FileIsNotPowerShellError(input));

        // 3. Launch Execution
        var executionOutput = await _powerShellExecutor.Execute(input.ScriptPath);

        // 4. Capture Execution Output
        var useCaseOutput = new ExecutePowerShellOutput(
            executionOutput.ExitCode,
            executionOutput.StdOut,
            executionOutput.StdErr
        );

        // 5. Based on Status - Return a Success or Failure result, wrapping the output.
        if (useCaseOutput.ExitCode != 0)
            return Result.Fail<ExecutePowerShellOutput>(
                new FailureExitCodeError(input, executionOutput.ExitCode, executionOutput.StdOut, executionOutput.StdErr)
            );

        return Result.Ok(useCaseOutput);
    }
}