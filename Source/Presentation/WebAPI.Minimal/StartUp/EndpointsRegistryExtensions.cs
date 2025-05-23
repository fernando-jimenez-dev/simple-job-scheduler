using System.Net;
using WebAPI.Minimal.Shared;
using WebAPI.Minimal.UseCases.CreateJobSchedule;
using WebAPI.Minimal.UseCases.ExecutePowerShell;

namespace WebAPI.Minimal.StartUp;

public static class EndpointsRegistryExtensions
{
    public static void RegisterWebApiEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.AddExecutePowerShellEndpoint();
        routes.AddCreateJobScheduleEndpoint();
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

    private static IEndpointRouteBuilder AddCreateJobScheduleEndpoint(this IEndpointRouteBuilder routes)
    {
        var groupName = "/schedule";
        var group = routes.MapGroup(groupName);

        group
            .MapPost("/new", CreateJobScheduleEndpoint.Execute)
            .WithName("CreateJobScheduleEndpoint")
            .WithTags("scheduling")
            .Produces<CreateJobScheduleResponse>((int)CreateJobScheduleResponse.SuccessCode)
            .Produces<ApiError>((int)CreateJobScheduleResponse.ValidationErrorCode)
            .Produces<ApiError>((int)CreateJobScheduleResponse.JobDoesNotExistErrorCode)
            .Produces<ApiError>((int)CreateJobScheduleResponse.FailedToSaveScheduleErrorCode)
            .Produces<ApiError>((int)HttpStatusCode.InternalServerError)
            ;

        return routes;
    }
}