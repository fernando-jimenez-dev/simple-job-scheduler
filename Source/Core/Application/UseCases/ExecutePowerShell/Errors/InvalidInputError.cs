using FluentResults;
using FluentValidation.Results;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace Application.UseCases.ExecutePowerShell.Errors;

public class InvalidInputError : Error
{
    public readonly ExecutePowerShellInput Input;

    public InvalidInputError(ExecutePowerShellInput input, ValidationResult validationResult) : base("Input is invalid")
    {
        Input = input;

        Metadata.Add("Guid", Guid.NewGuid());
        Metadata.Add("Type", nameof(InvalidInputError));
    }
}