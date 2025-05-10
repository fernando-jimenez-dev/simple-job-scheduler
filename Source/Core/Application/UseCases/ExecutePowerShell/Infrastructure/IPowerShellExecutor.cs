namespace Application.UseCases.ExecutePowerShell.Infrastructure;

public interface IPowerShellExecutor
{
    public Task<PowerShellExecutorOutput> Execute(string scriptPath);
}

public record PowerShellExecutorOutput(int ExitCode, string? StdOut = null, string? StdErr = null);