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
        logger.LogInformation("Received a request to execute the PowerShell script at: {Path}", request.ScriptPath);

        var input = new ExecutePowerShellInput(request.ScriptPath);
        var result = await useCase.Run(input, cancellationToken);

        var response = new ExecutePowerShellResponse(result, request);

        if (response.IsSuccess)
            logger.LogInformation("PowerShell script executed successfully.");
        else
            logger.Log(
                response.Error?.Severity ?? LogLevel.Error,
                response.Error?.Exception,
                "PowerShell script execution failed. Error: {Error}",
                response.Error is null ? "ApiError object was null." : response.Error.ToString()
            );

        logger.LogInformation("Returning response with HTTP status code: {StatusCode} ({Name})", response.StatusCode, response.HttpStatusCode);
        return Results.Json(data: response, statusCode: response.StatusCode);
    }
}