using Application.JobsUseCases.ExecutePowerShell;
using Application.JobsUseCases.ExecutePowerShell.Abstractions;
using Application.JobsUseCases.ExecutePowerShell.Infrastructure;
using Application.SchedulingUseCases.ScheduleJob;
using Application.SchedulingUseCases.ScheduleJob.Abstractions;
using Application.SchedulingUseCases.ScheduleJob.Infrastructure;
using System.IO.Abstractions;

namespace WebAPI.Minimal.StartUp;

/// <summary>
/// Contains extension methods for configuring dependency injection services.
/// Provides a centralized way to register all application dependencies.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures all dependencies for the application.
    /// Use this method to register services, middleware, and other components.
    /// </summary>
    /// <param name="services">The IServiceCollection to configure.</param>
    /// <returns>The configured IServiceCollection instance.</returns>
    public static IServiceCollection ConfigureWebApiDependencies(this IServiceCollection services)
    {
        return services
            .AddExecutePowerShellUseCase()
            .AddScheduleJobUseCase()
            ;
    }

    private static IServiceCollection AddExecutePowerShellUseCase(this IServiceCollection services)
    {
        // Use Case
        services.AddScoped<IExecutePowerShellUseCase, ExecutePowerShellUseCase>();

        // Infrastructure
        services.AddScoped<IPowerShellExecutor, PowerShellExecutor>();
        services.AddScoped<IScriptFileVerifier, ScriptFileVerifier>();
        services.AddScoped<IFileSystem, FileSystem>();

        return services;
    }

    private static IServiceCollection AddScheduleJobUseCase(this IServiceCollection services)
    {
        // Use Case
        services.AddScoped<IScheduleJobUseCase, ScheduleJobUseCase>();

        // Infrastructure
        services.AddScoped<IScheduleJobRepository, ScheduleJobInMemoryRepository>();

        return services;
    }
}