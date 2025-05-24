using Application.JobsUseCases.ExecutePowerShell;
using Application.JobsUseCases.ExecutePowerShell.Abstractions;
using Application.JobsUseCases.ExecutePowerShell.Errors;
using Application.Shared.Errors;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using static Application.JobsUseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;
using static Application.JobsUseCases.ExecutePowerShell.Abstractions.IPowerShellExecutor;

namespace Application.UnitTests.JobsUseCases.ExecutePowerShell;

public class ExecutePowerShellUseCaseTests
{
    private readonly ExecutePowerShellUseCase _useCase;
    private readonly IPowerShellExecutor _powerShellExecutor;
    private readonly IScriptFileVerifier _scriptFileVerifier;
    private readonly ILogger<ExecutePowerShellUseCase> _logger;

    public ExecutePowerShellUseCaseTests()
    {
        _powerShellExecutor = Substitute.For<IPowerShellExecutor>();
        _scriptFileVerifier = Substitute.For<IScriptFileVerifier>();
        _logger = Substitute.For<ILogger<ExecutePowerShellUseCase>>();
        _useCase = new ExecutePowerShellUseCase(_powerShellExecutor, _scriptFileVerifier, _logger);
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
        output.StandardOutput.ShouldBe("All went well!");
        output.ExitCode.ShouldBe(0);
        output.StandardError.ShouldBe("ERROR: Nothing too dramatic.");
    }

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

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldNotBeNull().Root;
        var failureExitCodeError = error.ShouldBeOfType<FailureExitCodeError>();
        failureExitCodeError.Input.ShouldBe(input);
        failureExitCodeError.ExitCode.ShouldBe(failingExitCode);
        failureExitCodeError.StandardOutput.ShouldBe("I was executing but...");
        failureExitCodeError.StandardError.ShouldBe("ERROR: Some description");
        failureExitCodeError.Id.ToString().ShouldNotBeNullOrEmpty();
        failureExitCodeError.Type.ShouldBe(nameof(FailureExitCodeError));
    }

    [Fact]
    public async Task ShouldFail_WhenFileCannotBeFound()
    {
        var input = new ExecutePowerShellInput("//path//to//script.ps1");
        _scriptFileVerifier.IsPowerShell(input.ScriptPath).Returns(true);
        _scriptFileVerifier.Exists(input.ScriptPath).Returns(false);

        var result = await _useCase.Run(input);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldNotBeNull().Root;
        var fileNotFoundError = error.ShouldBeOfType<FileNotFoundError>();
        fileNotFoundError.Input.ShouldBe(input);
        fileNotFoundError.Id.ToString().ShouldNotBeNullOrEmpty();
        fileNotFoundError.Type.ShouldBe(nameof(FileNotFoundError));
        await _powerShellExecutor.DidNotReceive().Execute(Arg.Any<string>());
    }

    [Fact]
    public async Task ShouldFail_WhenFileIsNotPowershellScript()
    {
        var input = new ExecutePowerShellInput("//path//to//script.notps1");
        _scriptFileVerifier.Exists(input.ScriptPath).Returns(true);
        _scriptFileVerifier.IsPowerShell(input.ScriptPath).Returns(false);

        var result = await _useCase.Run(input);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldNotBeNull().Root;
        var fileIsNotPsError = error.ShouldBeOfType<FileIsNotPowerShellError>();
        fileIsNotPsError.Input.ShouldBe(input);
        fileIsNotPsError.Id.ToString().ShouldNotBeNullOrEmpty();
        fileIsNotPsError.Type.ShouldBe(nameof(FileIsNotPowerShellError));
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

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldNotBeNull().Root;
        var invalidInputError = error.ShouldBeOfType<InvalidInputError>();
        invalidInputError.Input.ShouldBe(input);
        invalidInputError.Id.ToString().ShouldNotBeNullOrEmpty();
        invalidInputError.Type.ShouldBe(nameof(InvalidInputError));
        await _powerShellExecutor.DidNotReceive().Execute(Arg.Any<string>());
    }

    [Fact]
    public async Task ShouldFail_WhenUnexpectedExceptionIsThrown()
    {
        var input = new ExecutePowerShellInput("//path//to//script.ps1");
        _scriptFileVerifier.Exists(input.ScriptPath).Returns(true);
        _scriptFileVerifier.IsPowerShell(input.ScriptPath).Returns(true);

        var exception = new InvalidOperationException("Something unexpected happened.");
        _powerShellExecutor
            .Execute(input.ScriptPath, Arg.Any<CancellationToken>())
            .Throws(exception);

        var result = await _useCase.Run(input);

        result.IsFailure.ShouldBeTrue();
        var error = result.Error.ShouldNotBeNull().Root;
        var unexpectedError = error.ShouldBeOfType<UnexpectedError>();
        unexpectedError.Message.ShouldBe("An unexpected error ocurred while executing the PowerShell.");
        unexpectedError.Exception.ShouldBe(exception);
        unexpectedError.Id.ToString().ShouldNotBeNullOrEmpty();
        unexpectedError.Type.ShouldBe(nameof(UnexpectedError));
    }
}