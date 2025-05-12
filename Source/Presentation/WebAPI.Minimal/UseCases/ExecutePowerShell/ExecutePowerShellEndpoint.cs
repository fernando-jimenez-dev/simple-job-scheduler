using Application.UseCases.ExecutePowerShell.Abstractions;
using Microsoft.AspNetCore.Mvc;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace WebAPI.Minimal.UseCases.ExecutePowerShell;

public class ExecutePowerShellEndpoint
{
    public static async Task<IResult> Execute(
        [FromServices] IExecutePowerShellUseCase useCase,
        [FromServices] ILogger<ExecutePowerShellEndpoint> logger,
        [FromBody] ExecutePowerShellRequest request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("We are alive!");
        var input = new ExecutePowerShellInput(request.ScriptPath);
        var result = await useCase.Run(input, cancellationToken);
        var response = new ExecutePowerShellResponse(result, request);

        return Results.Json(data: response, statusCode: response.StatusCode);
    }
}