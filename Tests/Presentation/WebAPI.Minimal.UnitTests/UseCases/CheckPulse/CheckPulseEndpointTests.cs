﻿using Application.Shared.Errors;
using Application.UseCases.CheckPulse;
using Application.UseCases.CheckPulse.Abstractions;
using Application.UseCases.CheckPulse.Errors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;
using WebAPI.Minimal.UseCases.CheckPulse;

namespace WebAPI.Minimal.UnitTests.UseCases.CheckPulse;

public class CheckPulseEndpointTests
{
    private readonly ICheckPulseUseCase checkPulseUseCase;
    private readonly ILogger<CheckPulseEndpoint> logger;
    private readonly CancellationToken cancellationToken;

    public CheckPulseEndpointTests()
    {
        checkPulseUseCase = Substitute.For<ICheckPulseUseCase>();
        logger = Substitute.For<ILogger<CheckPulseEndpoint>>();
        cancellationToken = default;
    }

    [Fact]
    public async Task ShouldReturnOkWhenUseCaseSucceeds()
    {
        // Arrange
        checkPulseUseCase
            .Run(Arg.Any<string>(), cancellationToken)
            .Returns(new CheckPulseUseCaseOutput(isSuccess: true));

        // Act
        var endpointResult = await CheckPulseEndpoint.Execute(checkPulseUseCase, logger, cancellationToken);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<CheckPulseEndpointResponse>>(endpointResult);
        Assert.Equal((int)HttpStatusCode.OK, jsonResult.StatusCode);
        Assert.NotNull(jsonResult.Value);
        Assert.Equal("Pulse checked!", jsonResult.Value.Message);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenValidationErrorOccurs()
    {
        // Arrange
        checkPulseUseCase
            .Run(Arg.Any<string>(), cancellationToken)
            .Returns(new CheckPulseUseCaseOutput(new ValidationError("Invalid input", "Input")));

        // Act
        var endpointResult = await CheckPulseEndpoint.Execute(checkPulseUseCase, logger, cancellationToken);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<CheckPulseEndpointResponse>>(endpointResult);
        Assert.Equal((int)HttpStatusCode.BadRequest, jsonResult.StatusCode);
        Assert.NotNull(jsonResult.Value);
        Assert.Equal("Invalid input", jsonResult.Value.Message);
    }

    [Fact]
    public async Task ShouldReturnInternalServerErrorForEmptyVitalsError()
    {
        // Arrange
        checkPulseUseCase
            .Run(Arg.Any<string>(), cancellationToken)
            .Returns(new CheckPulseUseCaseOutput(new EmptyVitalsError()));

        // Act
        var endpointResult = await CheckPulseEndpoint.Execute(checkPulseUseCase, logger, cancellationToken);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<CheckPulseEndpointResponse>>(endpointResult);
        Assert.Equal((int)HttpStatusCode.InternalServerError, jsonResult.StatusCode);
        Assert.NotNull(jsonResult.Value);
        Assert.Equal("No vitals were found.", jsonResult.Value.Message);
    }

    [Fact]
    public async Task ShouldReturnInternalServerErrorForUnexpectedError()
    {
        // Arrange
        checkPulseUseCase
            .Run(Arg.Any<string>(), cancellationToken)
            .Returns(new CheckPulseUseCaseOutput(new UnexpectedError("Unexpected issue occurred.", new ApplicationException())));

        // Act
        var endpointResult = await CheckPulseEndpoint.Execute(checkPulseUseCase, logger, cancellationToken);

        // Assert
        var statusResult = Assert.IsType<StatusCodeHttpResult>(endpointResult);
        Assert.Equal((int)HttpStatusCode.InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnInternalServerErrorWhenUseCaseThrowsException()
    {
        // Arrange
        checkPulseUseCase
            .Run(Arg.Any<string>(), cancellationToken)
            .Throws(new ApplicationException("Critical failure"));

        // Act
        var endpointResult = await CheckPulseEndpoint.Execute(checkPulseUseCase, logger, cancellationToken);

        // Assert
        var jsonResult = Assert.IsType<JsonHttpResult<CheckPulseEndpointResponse>>(endpointResult);
        Assert.Equal((int)HttpStatusCode.InternalServerError, jsonResult.StatusCode);
        Assert.NotNull(jsonResult.Value);
        Assert.Equal("Unrecoverable error encountered.", jsonResult.Value.Message);
    }
}