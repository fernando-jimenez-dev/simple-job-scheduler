using Application.JobsUseCases.ExecutePowerShell.Infrastructure;
using NSubstitute;
using Shouldly;
using System.IO.Abstractions;

namespace Application.UnitTests.JobsUseCases.ExecutePowerShell.Infrastructure;

public class ScriptFileVerifierTests
{
    private readonly IFileSystem _fileSystem;
    private readonly ScriptFileVerifier _scriptFileVerifier;

    public ScriptFileVerifierTests()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _scriptFileVerifier = new ScriptFileVerifier(_fileSystem);
    }

    [Fact]
    public void Exists_ShouldBeTrue_WhenFileExists()
    {
        var path = Path.Combine("path/to/script.ps1");
        _fileSystem.File.Exists(path).Returns(true);

        var exists = _scriptFileVerifier.Exists(path);

        exists.ShouldBeTrue();
    }

    [Fact]
    public void Exists_ShouldBeTrue_WhenFileDoesNotExist()
    {
        var path = Path.Combine("path/to/script.ps1");
        _fileSystem.File.Exists(path).Returns(false);

        var exists = _scriptFileVerifier.Exists(path);

        exists.ShouldBeFalse();
    }

    [Theory]
    [InlineData(".ps1")]
    [InlineData(".PS1")]
    [InlineData(".pS1")]
    [InlineData(".Ps1")]
    public void IsPowerShell_ShouldBeTrue_WhenFileHasProperExtension(string fileExtension)
    {
        var path = Path.Combine("path", "to", "script", fileExtension);
        _fileSystem.Path.GetExtension(path).Returns(fileExtension);

        var isPowerShell = _scriptFileVerifier.IsPowerShell(path);

        isPowerShell.ShouldBeTrue();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".ps11")]
    [InlineData(".tps1")]
    [InlineData(".ps")]
    [InlineData("ps1")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void IsPowerShell_ShouldBeFalse_WhenFileHasInvalidExtension(string fileExtension)
    {
        var path = Path.Combine("path", "to", $"script{fileExtension}");
        _fileSystem.Path.GetExtension(path).Returns(fileExtension);

        var isPowerShell = _scriptFileVerifier.IsPowerShell(path);

        isPowerShell.ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void IsPowerShell_ShouldBeFalse_WhenFilePathIsEmpty(string invalidPath)
    {
        var isPowerShell = _scriptFileVerifier.IsPowerShell(invalidPath);

        isPowerShell.ShouldBeFalse();
    }
}