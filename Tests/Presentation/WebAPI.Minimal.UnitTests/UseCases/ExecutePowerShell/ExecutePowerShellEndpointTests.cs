using Application.Shared.Errors;
using Application.UseCases.ExecutePowerShell.Abstractions;
using Application.UseCases.ExecutePowerShell.Errors;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenResult;
using Shouldly;
using System.Net;
using WebAPI.Minimal.UseCases.ExecutePowerShell;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace WebAPI.Minimal.UnitTests.UseCases.ExecutePowerShell;

public class ExecutePowerShellEndpointTests
{
    private readonly IExecutePowerShellUseCase _useCase;
    private readonly ILogger<ExecutePowerShellEndpoint> _logger;

    public ExecutePowerShellEndpointTests()
    {
        _useCase = Substitute.For<IExecutePowerShellUseCase>();
        _logger = Substitute.For<ILogger<ExecutePowerShellEndpoint>>();
    }

    [Theory]
    [MemberData(nameof(UseCaseResults))]
    public async Task ShouldReturnJsonResponse_WithMappedResponseAndStatus(
        Result<ExecutePowerShellOutput> result
    )
    {
        var request = new ExecutePowerShellRequest("some-path");
        _useCase.Run(Arg.Any<ExecutePowerShellInput>(), Arg.Any<CancellationToken>())
            .Returns(result);

        var endpointResult = await ExecutePowerShellEndpoint.Execute(_useCase, _logger, request, CancellationToken.None);

        var expectedResponse = new ExecutePowerShellResponse(result, request);
        var json = endpointResult.ShouldBeOfType<JsonHttpResult<ExecutePowerShellResponse>>();
        json.StatusCode.ShouldBe(expectedResponse.StatusCode);
        json.Value.ShouldBeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ShouldReturnJsonResponse_WhenUnexpectedErrorIsReturned()
    {
        var request = new ExecutePowerShellRequest("some-path");

        var unexpectedErrorResult = Result<ExecutePowerShellOutput>.Failure(
            new UnexpectedError("Something went wrong")
        );
        _useCase.Run(Arg.Any<ExecutePowerShellInput>(), Arg.Any<CancellationToken>())
            .Returns(unexpectedErrorResult);

        var endpointResult = await ExecutePowerShellEndpoint.Execute(_useCase, _logger, request, CancellationToken.None);

        var json = endpointResult.ShouldBeOfType<JsonHttpResult<ExecutePowerShellResponse>>();
        json.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        json.Value.ShouldNotBeNull();
        json.Value.Error.ShouldNotBeNull();
        json.Value.Error.Type.ShouldBe("UnexpectedError");
        json.Value.Error.Message.ShouldBe("Something went wrong");
        json.Value.Error.Id.ShouldNotBe(Guid.Empty);
    }

    public static IEnumerable<object[]> UseCaseResults()
    {
        yield return new object[]
        {
        Result.Success(new ExecutePowerShellOutput(0, "Everything fine", ""))
        };

        yield return new object[]
        {
        Result<ExecutePowerShellOutput>.Failure(
            new FileNotFoundError(new("missing.ps1"))
        )
        };

        yield return new object[]
        {
        Result<ExecutePowerShellOutput>.Failure(
            new FileIsNotPowerShellError(new("invalid.txt"))
        )
        };

        yield return new object[]
        {
        Result<ExecutePowerShellOutput>.Failure(
            new InvalidInputError(new(""), new ValidationResult([
                new ValidationFailure("property", "invalid value")
            ]))
        )
        };

        yield return new object[]
        {
        Result<ExecutePowerShellOutput>.Failure(
            new FailureExitCodeError(new("fail.ps1"), 1, "bad output", "stderr info")
        )
        };
    }
}