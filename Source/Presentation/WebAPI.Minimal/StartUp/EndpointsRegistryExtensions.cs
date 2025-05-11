using WebAPI.Minimal.UseCases.CheckPulse;
using WebAPI.Minimal.UseCases.ExecutePowerShell;

namespace WebAPI.Minimal.StartUp;

public static class EndpointsRegistryExtensions
{
    public static void RegisterWebApiEndpoints(this IEndpointRouteBuilder routes)
    {
        //routes.AddCheckPulseEndpoint();
        routes.AddExecutePowerShellEndpoint();
    }

    private static IEndpointRouteBuilder AddCheckPulseEndpoint(this IEndpointRouteBuilder routes)
    {
        var groupName = "/check-pulse";
        var group = routes.MapGroup(groupName);

        group
            .MapGet("/", CheckPulseEndpoint.Execute)
            .WithName("CheckPulse")
            .WithOpenApi();

        return routes;
    }

    private static IEndpointRouteBuilder AddExecutePowerShellEndpoint(this IEndpointRouteBuilder routes)
    {
        var groupName = "/powershell";
        var group = routes.MapGroup(groupName);

        group
            .MapPost("/execute", ExecutePowerShellEndpoint.Execute)
            .WithName("ExecutePowerShell")
            .WithOpenApi();

        return routes;
    }
}