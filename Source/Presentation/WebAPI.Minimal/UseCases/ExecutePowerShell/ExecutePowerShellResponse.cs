using Application.UseCases.ExecutePowerShell.Abstractions;
using FluentResults;

namespace WebAPI.Minimal.UseCases.ExecutePowerShell;

public record ExecutePowerShellResponse(
    Result<IExecutePowerShellUseCase.ExecutePowerShellOutput> UseCaseResult
);