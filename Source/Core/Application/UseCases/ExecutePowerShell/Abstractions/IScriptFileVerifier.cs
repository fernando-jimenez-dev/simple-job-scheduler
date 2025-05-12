namespace Application.UseCases.ExecutePowerShell.Abstractions;

public interface IScriptFileVerifier
{
    /// <summary>
    /// Verifies if the script exists in the path.
    /// </summary>
    bool Exists(string path);

    /// <summary>
    /// Verifies that the file path is indeed a PowerShell script path.
    /// Verification is done verifying the .ps1 extension only.
    /// </summary>
    bool IsPowerShell(string path);
}