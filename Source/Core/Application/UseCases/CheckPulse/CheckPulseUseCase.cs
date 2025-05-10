using Application.Shared.Errors;
using Application.UseCases.CheckPulse.Abstractions;
using Application.UseCases.CheckPulse.Errors;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.CheckPulse;

/// <summary>
/// A simple use case to verify the application's operational state.
/// Use this class as a reference to start implementing your very own use cases.
/// </summary>
/// <remarks>
/// This template serves as a baseline for implementing basic "pulse check"
/// within the system. It logs a confirmation message along with the
/// provided input and returns a successful result.
/// </remarks>
public class CheckPulseUseCase : ICheckPulseUseCase
{
    private readonly ILogger<CheckPulseUseCase> logger;
    private readonly ICheckPulseRepository checkPulseRepository;

    public CheckPulseUseCase(ILogger<CheckPulseUseCase> logger, ICheckPulseRepository checkPulseRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.checkPulseRepository = checkPulseRepository ?? throw new ArgumentNullException(nameof(checkPulseRepository));
    }

    //public async Task<CheckPulseUseCaseOutput> Run(string input, CancellationToken cancellationToken = default)
    //{
    //            if (string.IsNullOrWhiteSpace(input))
    //            return ValidationErrorOutput("Input cannot be empty", "CheckPulseUseCase Input");

    //        try
    //        {
    //            return await HandlePulseCheck(input, cancellationToken);
    //}
    //        catch (Exception exception)
    //        {
    //            return UnexpectedErrorOutput(input, exception);
    //        }
    //}

    public Task<Result<CheckPulseUseCaseOutput>> Run(string input, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task<CheckPulseUseCaseOutput> HandlePulseCheck(string input, CancellationToken cancellationToken)
    {
        var vitalReadings = await checkPulseRepository.RetrieveVitalReadings(cancellationToken);

        if (vitalReadings.Length > 0)
        {
            logger.LogInformation("System operational. Input: {Input}", input);
            await checkPulseRepository.SaveNewVitalCheck();
            return new CheckPulseUseCaseOutput(isSuccess: true);
        }

        logger.LogDebug("No vital readings found.");
        return new CheckPulseUseCaseOutput(new EmptyVitalsError());
    }

    private CheckPulseUseCaseOutput ValidationErrorOutput(string message, string field)
    {
        logger.LogDebug("Validation error: {Message}", message);
        return new CheckPulseUseCaseOutput(new ValidationError(message, field));
    }

    private CheckPulseUseCaseOutput UnexpectedErrorOutput(string input, Exception exception)
    {
        var errorMessage = $"Unexpected error during Check Pulse use case. Input: '{input}'";
        logger.LogError(exception, errorMessage);
        return new CheckPulseUseCaseOutput(new UnexpectedError(errorMessage, exception));
    }
}