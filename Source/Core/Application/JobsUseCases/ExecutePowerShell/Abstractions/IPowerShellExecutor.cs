namespace Application.JobsUseCases.ExecutePowerShell.Abstractions;

public interface IPowerShellExecutor
{
    public Task<PowerShellExecutorOutput> Execute(string scriptPath, CancellationToken cancellationToken = default);

    public record PowerShellExecutorOutput(int ExitCode, string? StdOut = null, string? StdErr = null);
}