using Application.UseCases.ExecutePowerShell.Abstractions;
using System.Diagnostics;
using static Application.UseCases.ExecutePowerShell.Abstractions.IPowerShellExecutor;

namespace Application.UseCases.ExecutePowerShell.Infrastructure;

public class PowerShellExecutor : IPowerShellExecutor
{
    /// <summary>
    /// Assumptions:
    /// -> scriptPath has been validated to be an existing powershell script.
    /// </summary>
    public async Task<PowerShellExecutorOutput> Execute(string scriptPath, CancellationToken cancellationToken = default)
    {
        var processInformation = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processInformation };

        process.Start();

        var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await Task.WhenAll(stdOutTask, stdErrTask);
        await process.WaitForExitAsync(cancellationToken);

        return new PowerShellExecutorOutput(process.ExitCode, stdOutTask.Result, stdErrTask.Result);
    }
}