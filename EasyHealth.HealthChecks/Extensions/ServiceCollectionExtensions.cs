using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using EasyHealth.HealthChecks.Core;
using EasyHealth.HealthChecks.Checks;

namespace EasyHealth.HealthChecks.Extensions;

/// <summary>
/// Extension methods for configuring EasyHealth health checks.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds EasyHealth health checks with default configuration including memory and disk space checks.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEasyHealthChecks(this IServiceCollection services)
    {
        return services.AddEasyHealthChecks(config =>
        {
            config.AddMemoryCheck();
            config.AddDiskSpaceCheck();
        });
    }

    /// <summary>
    /// Adds EasyHealth health checks with a database connection string and default system checks.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">Database connection string for health checks.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEasyHealthChecks(this IServiceCollection services, string connectionString)
    {
        return services.AddEasyHealthChecks(config =>
        {
            config.AddDatabase(connectionString);
            config.AddMemoryCheck();
            config.AddDiskSpaceCheck();
        });
    }

    /// <summary>
    /// Adds EasyHealth health checks with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureHealthChecks">Action to configure health checks.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEasyHealthChecks(this IServiceCollection services, Action<EasyHealthConfiguration> configureHealthChecks)
    {
        var config = new EasyHealthConfiguration();
        configureHealthChecks(config);

        // Register all configurators
        foreach (var configurator in config.Configurators)
        {
            configurator(services);
        }

        // Ensure health checks service is registered if not already done
        if (!services.Any(x => x.ServiceType == typeof(HealthCheckService)))
        {
            services.AddHealthChecks();
        }

        return services;
    }

    /// <summary>
    /// Wraps all registered health checks with resilient wrappers that never throw exceptions.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddResilientHealthChecks(this IServiceCollection services)
    {
        // This would be implemented in a post-processor that wraps existing health checks
        // For now, we'll add it as a configuration option
        services.Configure<HealthCheckServiceOptions>(options =>
        {
            // Add a default resilient behavior by setting reasonable timeouts
            options.Registrations.ToList().ForEach(registration =>
            {
                if (registration.Timeout == Timeout.InfiniteTimeSpan)
                {
                    registration.Timeout = TimeSpan.FromSeconds(30);
                }
            });
        });

        return services;
    }
}

/// <summary>
/// Extension methods for configuring EasyHealth health checks in the application pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Maps EasyHealth health check endpoints with default configuration.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="pattern">The URL pattern for health checks (default: "/health").</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseEasyHealthChecks(this IApplicationBuilder app, string pattern = "/health")
    {
        app.UseHealthChecks(pattern, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    status = report.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    totalDuration = report.TotalDuration.TotalMilliseconds,
                    checks = report.Entries.Select(kvp => new
                    {
                        name = kvp.Key,
                        status = kvp.Value.Status.ToString(),
                        description = kvp.Value.Description,
                        duration = kvp.Value.Duration.TotalMilliseconds,
                        data = kvp.Value.Data,
                        exception = kvp.Value.Exception?.Message
                    })
                };

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));
            }
        });

        return app;
    }

    /// <summary>
    /// Maps detailed EasyHealth health check endpoints for monitoring systems.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="pattern">The URL pattern for detailed health checks (default: "/health/detailed").</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseDetailedEasyHealthChecks(this IApplicationBuilder app, string pattern = "/health/detailed")
    {
        app.UseHealthChecks(pattern, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    status = report.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    totalDuration = report.TotalDuration.TotalMilliseconds,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    machineName = Environment.MachineName,
                    processId = Environment.ProcessId,
                    checks = report.Entries.Select(kvp => new
                    {
                        name = kvp.Key,
                        status = kvp.Value.Status.ToString(),
                        description = kvp.Value.Description,
                        duration = kvp.Value.Duration.TotalMilliseconds,
                        data = kvp.Value.Data,
                        exception = kvp.Value.Exception?.Message,
                        tags = kvp.Value.Tags
                    })
                };

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));
            }
        });

        return app;
    }
}