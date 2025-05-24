using Application.JobsUseCases.ExecutePowerShell.Abstractions;
using System.IO.Abstractions;

namespace Application.JobsUseCases.ExecutePowerShell.Infrastructure;

public class ScriptFileVerifier : IScriptFileVerifier
{
    private readonly IFileSystem _fileSystem;

    public ScriptFileVerifier(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public bool Exists(string path) => _fileSystem.File.Exists(path);

    public bool IsPowerShell(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;

        var pathExtension = _fileSystem.Path.GetExtension(path);

        if (pathExtension is null) return false;
        if (!pathExtension.Equals(".ps1", StringComparison.OrdinalIgnoreCase)) return false;

        return true;
    }
}