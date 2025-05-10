using Application.Shared.Abstractions.UseCase;
using FluentValidation;
using FluentValidation.Results;

namespace Application.UseCases.ExecutePowerShell.Abstractions;

public interface IExecutePowerShellUseCase : IUseCase
    <
    IExecutePowerShellUseCase.ExecutePowerShellInput,
    IExecutePowerShellUseCase.ExecutePowerShellOutput
    >
{
    /// <param name="ScriptPath">Path to the Powershell Script in the local computer.</param>
    public record ExecutePowerShellInput(string ScriptPath)
    {
        public ValidationResult Validate() => new Validator().Validate(this);

        public class Validator : AbstractValidator<ExecutePowerShellInput>
        {
            public Validator()
            {
                RuleFor(x => x.ScriptPath).NotEmpty().NotNull();
            }
        }
    }
    /// <param name="ExitCode">PowerShell exit code.</param>
    /// <param name="ScriptOutput">Powershell Standard Output (stdout).</param>
    /// <param name="Error">Powershell Standard Error (stderr).</param>
    public record ExecutePowerShellOutput(int ExitCode, string? ScriptOutput = null, string? Error = null);
}