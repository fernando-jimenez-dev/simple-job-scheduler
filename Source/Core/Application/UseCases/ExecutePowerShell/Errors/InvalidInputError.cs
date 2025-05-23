using Application.Shared.Errors;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.UseCases.ExecutePowerShell.Errors;

public record InvalidInputError : ApplicationError
{
    public ExecutePowerShellInput Input { get; }
    public string ValidationMessage { get; }
    public override LogLevel Severity => LogLevel.Warning;

    public InvalidInputError
        (ExecutePowerShellInput input, ValidationResult validationResult)
        : base(nameof(InvalidInputError), "Input is invalid")
    {
        Input = input;
        ValidationMessage = validationResult.ToString();
    }
}