using System.Net;
using WebAPI.Minimal.Shared;
using WebAPI.Minimal.JobsUseCases.ExecutePowerShell;
using WebAPI.Minimal.SchedulingUseCases.ScheduleJob;

namespace WebAPI.Minimal.StartUp;

public static class EndpointsRegistryExtensions
{
    public static void RegisterWebApiEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.AddExecutePowerShellEndpoint();
        routes.AddScheduleJobEndpoint();
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

    private static IEndpointRouteBuilder AddScheduleJobEndpoint(this IEndpointRouteBuilder routes)
    {
        var groupName = "/schedule";
        var group = routes.MapGroup(groupName);

        group
            .MapPost("/new", ScheduleJobEndpoint.Execute)
            .WithName("ScheduleJobEndpoint")
            .WithTags("scheduling")
            .Produces<ScheduleJobResponse>((int)ScheduleJobResponse.SuccessCode)
            .Produces<ApiError>((int)ScheduleJobResponse.ValidationErrorCode)
            .Produces<ApiError>((int)ScheduleJobResponse.JobDoesNotExistErrorCode)
            .Produces<ApiError>((int)ScheduleJobResponse.FailedToSaveScheduleErrorCode)
            .Produces<ApiError>((int)HttpStatusCode.InternalServerError)
            ;

        return routes;
    }
}