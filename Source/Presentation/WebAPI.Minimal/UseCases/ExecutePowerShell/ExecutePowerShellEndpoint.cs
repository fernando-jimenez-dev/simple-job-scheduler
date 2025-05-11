using Application.UseCases.ExecutePowerShell.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static Application.UseCases.ExecutePowerShell.Abstractions.IExecutePowerShellUseCase;

namespace WebAPI.Minimal.UseCases.ExecutePowerShell;

public class ExecutePowerShellEndpoint
{
    public static async Task<IResult> Execute(
        [FromServices] IExecutePowerShellUseCase useCase,
        [FromBody] ExecutePowerShellRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var input = new ExecutePowerShellInput(request.ScriptPath);
            var result = await useCase.Run(input, cancellationToken);

            if (result.IsSuccess)
            {
                var response = new ExecutePowerShellResponse(result.Value);
                return Results.Json(data: response, statusCode: (int)HttpStatusCode.OK);
            }

            // Just 500 for now. Need to specialize later.
            return Results.Json(data: result, statusCode: (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception exception)
        {
            return Results.Json(data: null, statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
}