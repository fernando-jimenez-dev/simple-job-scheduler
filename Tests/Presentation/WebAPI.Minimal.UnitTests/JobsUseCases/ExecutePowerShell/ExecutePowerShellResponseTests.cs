using Application.JobsUseCases.ExecutePowerShell.Errors;
using Application.Shared.Errors;
using FluentValidation.Results;
using OpenResult;
using Shouldly;
using System.Net;
using WebAPI.Minimal.JobsUseCases.ExecutePowerShell;
using static Application.JobsUseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace WebAPI.Minimal.UnitTests.JobsUseCases.ExecutePowerShell;

public class ExecutePowerShellResponseTests
{
    [Fact]
    public void ShouldSucceed_WhenUseCaseResultIsSuccessful()
    {
        var useCaseOutput = new ExecutePowerShellOutput(0, "Output", null);
        var result = Result.Success(useCaseOutput);
        var request = new ExecutePowerShellRequest("path");

        var response = new ExecutePowerShellResponse(result, request);

        response.IsSuccess.ShouldBeTrue();
        response.Data.ShouldBe(useCaseOutput);
        response.Error.ShouldBeNull();
        response.HttpStatusCode.ShouldBe(HttpStatusCode.OK);
        response.StatusCode.ShouldBe((int)HttpStatusCode.OK);
    }

    [Fact]
    public void ShouldFailWithUnprocessableEntity_WhenFailureExitCodeErrorIsReturned()
    {
        var input = new ExecutePowerShellInput("bad-path");
        var error = new FailureExitCodeError(input, 1, "output", "stderr");
        var result = Result<ExecutePowerShellOutput>.Failure(error);
        var request = new ExecutePowerShellRequest(input.ScriptPath);

        var response = new ExecutePowerShellResponse(result, request);

        response.IsSuccess.ShouldBeFalse();
        response.Data.ShouldBeNull();
        response.Error.ShouldNotBeNull();
        response.Error.Type.ShouldBe(nameof(FailureExitCodeError));
        response.HttpStatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        response.Error.Details["exitCode"].ShouldBe(1);
        response.Error.Details["stdout"].ShouldBe("output");
        response.Error.Details["stderr"].ShouldBe("stderr");
    }

    [Fact]
    public void ShouldFailWithNotFound_WhenFileNotFoundErrorIsReturned()
    {
        var input = new ExecutePowerShellInput("notfound");
        var error = new FileNotFoundError(input);
        var result = Result<ExecutePowerShellOutput>.Failure(error);
        var request = new ExecutePowerShellRequest(input.ScriptPath);

        var response = new ExecutePowerShellResponse(result, request);

        response.IsSuccess.ShouldBeFalse();
        response.Error.ShouldNotBeNull();
        response.Error.Type.ShouldBe(nameof(FileNotFoundError));
        response.HttpStatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Data.ShouldBeNull();
    }

    [Fact]
    public void ShouldFailWithBadRequest_WhenFileIsNotPowerShellErrorIsReturned()
    {
        var input = new ExecutePowerShellInput("not-a-ps");
        var error = new FileIsNotPowerShellError(input);
        var result = Result<ExecutePowerShellOutput>.Failure(error);
        var request = new ExecutePowerShellRequest(input.ScriptPath);

        var response = new ExecutePowerShellResponse(result, request);

        response.IsSuccess.ShouldBeFalse();
        response.Error.ShouldNotBeNull();
        response.Error.Type.ShouldBe(nameof(FileIsNotPowerShellError));
        response.HttpStatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Data.ShouldBeNull();
    }

    [Fact]
    public void ShouldFailWithBadRequest_WhenInvalidInputErrorIsReturned()
    {
        var input = new ExecutePowerShellInput("bad");
        var validationResult = new ValidationResult([
            new ValidationFailure("property", "Invalid format")
        ]);
        var error = new InvalidInputError(input, validationResult);
        var result = Result<ExecutePowerShellOutput>.Failure(error);
        var request = new ExecutePowerShellRequest(input.ScriptPath);

        var response = new ExecutePowerShellResponse(result, request);

        response.IsSuccess.ShouldBeFalse();
        response.Error.ShouldNotBeNull();
        response.Error.Type.ShouldBe(nameof(InvalidInputError));
        response.HttpStatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Error.Details["validationMessage"].ShouldBe("Invalid format");
        response.Data.ShouldBeNull();
    }

    [Fact]
    public void ShouldFailWithInternalServerError_WhenUnexpectedErrorTypeIsReturned()
    {
        var result = Result<ExecutePowerShellOutput>.Failure(
            new ApplicationError("UnexpectedError", "Something bad")
        );
        var request = new ExecutePowerShellRequest("some-input");

        var response = new ExecutePowerShellResponse(result, request);

        response.IsSuccess.ShouldBeFalse();
        response.Data.ShouldBeNull();
        response.Error.ShouldNotBeNull();
        response.Error.Type.ShouldBe("UnexpectedError");
        response.HttpStatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void ShouldFailWithInternalServerError_WhenUnknownErrorTypeIsReturned()
    {
        var result = Result<ExecutePowerShellOutput>.Failure(
            new Error("This is not a known error")
        );
        var request = new ExecutePowerShellRequest("some-input");

        var response = new ExecutePowerShellResponse(result, request);

        response.IsSuccess.ShouldBeFalse();
        response.Data.ShouldBeNull();
        response.Error.ShouldNotBeNull();
        response.Error.Type.ShouldBe("UnexpectedError");
        response.Error.Message.ShouldBe("An unexpected and unknown error ocurred.");
        response.HttpStatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }
}