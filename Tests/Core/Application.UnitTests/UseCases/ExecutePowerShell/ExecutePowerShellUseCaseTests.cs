using Application.UseCases.ExecutePowerShell;
using Application.UseCases.ExecutePowerShell.Abstractions;
using Application.UseCases.ExecutePowerShell.Errors;
using NSubstitute;
using Shouldly;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.UnitTests.UseCases.ExecutePowerShell;

public class ExecutePowerShellUseCaseTests
{
    private readonly ExecutePowerShellUseCase _useCase;
    private readonly IPowerShellExecutor _powerShellExecutor;
    private readonly IScriptFileVerifier _scriptFileVerifier;

    public ExecutePowerShellUseCaseTests()
    {
        _powerShellExecutor = Substitute.For<IPowerShellExecutor>();
        _scriptFileVerifier = Substitute.For<IScriptFileVerifier>();
        _useCase = new ExecutePowerShellUseCase(_powerShellExecutor, _scriptFileVerifier);
    }

    [Fact]
    public async Task ShouldSucceed_WhenScriptExecutesCorrectly()
    {
        var input = new ExecutePowerShellInput("//path//to//script.ps1");
        _scriptFileVerifier.Exists(input.ScriptPath).Returns(true);
        _scriptFileVerifier.IsPowerShell(input.ScriptPath).Returns(true);
        _powerShellExecutor
            .Execute(input.ScriptPath)
            .Returns(new PowerShellExecutorOutput(ExitCode: 0, StdOut: "All went well!", StdErr: "ERROR: Nothing too dramatic."));

        var result = await _useCase.Run(input);

        result.IsSuccess.ShouldBeTrue();
        var output = result.Value.ShouldNotBeNull();
        output.ScriptOutput.ShouldBe("All went well!");
        output.ExitCode.ShouldBe(0);
        output.Error.ShouldBe("ERROR: Nothing too dramatic.");
    }

    /// <summary>
    /// If the Exit Code is a Failure, we fail the entire use case execution.
    /// In this case, a failure represents any ExitCode that is not zero (0).
    /// </summary>
    [Fact]
    public async Task ShouldFail_WhenExitCodeIsAFailure()
    {
        var input = new ExecutePowerShellInput("//path//to//script.ps1");
        var failingExitCode = new Random().Next(1, int.MaxValue);
        _scriptFileVerifier.Exists(input.ScriptPath).Returns(true);
        _scriptFileVerifier.IsPowerShell(input.ScriptPath).Returns(true);
        _powerShellExecutor
            .Execute(input.ScriptPath)
            .Returns(new PowerShellExecutorOutput(ExitCode: failingExitCode, StdOut: "I was executing but...", StdErr: "ERROR: Some description"));

        var result = await _useCase.Run(input);

        result.IsFailed.ShouldBeTrue();
        var error = result.Errors.First();
        var failureExitCodeError = error.ShouldBeOfType<FailureExitCodeError>();
        failureExitCodeError.Input.ShouldBe(input);
        failureExitCodeError.ExitCode.ShouldBe(failingExitCode);
        failureExitCodeError.StandardOutput.ShouldBe("I was executing but...");
        failureExitCodeError.StandardError.ShouldBe("ERROR: Some description");
        failureExitCodeError.Metadata["Guid"].ShouldNotBeNull();
        failureExitCodeError.Metadata["Type"].ShouldBe(nameof(FailureExitCodeError));
    }

    [Fact]
    public async Task ShouldFail_WhenFileCannotBeFound()
    {
        var input = new ExecutePowerShellInput("//path//to//script.ps1");
        _scriptFileVerifier.Exists(input.ScriptPath).Returns(false);

        var result = await _useCase.Run(input);

        result.IsFailed.ShouldBeTrue();
        var error = result.Errors.First();
        var fileNotFoundError = error.ShouldBeOfType<FileNotFoundError>();
        fileNotFoundError.Input.ShouldBe(input);
        fileNotFoundError.Metadata["Guid"].ShouldNotBeNull();
        fileNotFoundError.Metadata["Type"].ShouldBe(nameof(FileNotFoundError));
        await _powerShellExecutor.DidNotReceive().Execute(Arg.Any<string>());
    }

    [Fact]
    public async Task ShouldFail_WhenFileIsNotPowershellScript()
    {
        var input = new ExecutePowerShellInput("//path//to//script.notps1");
        _scriptFileVerifier.Exists(input.ScriptPath).Returns(true);
        _scriptFileVerifier.IsPowerShell(input.ScriptPath).Returns(false);

        var result = await _useCase.Run(input);

        result.IsFailed.ShouldBeTrue();
        var error = result.Errors.First();
        var fileNotFoundError = error.ShouldBeOfType<FileIsNotPowerShellError>();
        fileNotFoundError.Input.ShouldBe(input);
        fileNotFoundError.Metadata["Guid"].ShouldNotBeNull();
        fileNotFoundError.Metadata["Type"].ShouldBe(nameof(FileIsNotPowerShellError));
        await _powerShellExecutor.DidNotReceive().Execute(Arg.Any<string>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ShouldFail_WhenInputIsInvalid(string invalidPath)
    {
        var input = new ExecutePowerShellInput(invalidPath);

        var result = await _useCase.Run(input);

        result.IsFailed.ShouldBeTrue();
        var error = result.Errors.First();
        var fileNotFoundError = error.ShouldBeOfType<InvalidInputError>();
        fileNotFoundError.Input.ShouldBe(input);
        fileNotFoundError.Metadata["Guid"].ShouldNotBeNull();
        fileNotFoundError.Metadata["Type"].ShouldBe(nameof(InvalidInputError));
        await _powerShellExecutor.DidNotReceive().Execute(Arg.Any<string>());
    }
}