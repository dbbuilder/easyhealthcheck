using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyHealth.HealthChecks.Core;

/// <summary>
/// Decorator that wraps health check registrations with resilient behavior.
/// </summary>
public static class HealthCheckDecorator
{
    /// <summary>
    /// Wraps all registered health checks with resilient wrappers.
    /// </summary>
    public static IServiceCollection MakeHealthChecksResilient(this IServiceCollection services)
    {
        // Decorate the HealthCheckService to wrap all health checks
        services.Decorate<HealthCheckService>((inner, provider) =>
        {
            var logger = provider.GetRequiredService<ILogger<ResilientHealthCheckService>>();
            return new ResilientHealthCheckService(inner, logger);
        });

        return services;
    }
}

/// <summary>
/// A resilient wrapper around the standard HealthCheckService that ensures all health checks
/// are wrapped with resilient behavior.
/// </summary>
public class ResilientHealthCheckService : HealthCheckService
{
    private readonly HealthCheckService _innerService;
    private readonly ILogger<ResilientHealthCheckService> _logger;

    public ResilientHealthCheckService(HealthCheckService innerService, ILogger<ResilientHealthCheckService> logger)
    {
        _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<HealthReport> CheckHealthAsync(
        Func<HealthCheckRegistration, bool>? predicate = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use a timeout to ensure we always return a result
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var report = await _innerService.CheckHealthAsync(predicate, combinedCts.Token);
            return report;
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Health check service timed out or was cancelled");
            
            // Return a degraded report instead of throwing
            var entries = new Dictionary<string, HealthReportEntry>
            {
                ["timeout"] = new HealthReportEntry(
                    HealthStatus.Degraded, 
                    "Health check service timed out but service remains operational", 
                    TimeSpan.Zero, 
                    ex, 
                    null)
            };

            return new HealthReport(entries, TimeSpan.Zero);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check service failed with unexpected error");
            
            // Return a degraded report instead of throwing
            var entries = new Dictionary<string, HealthReportEntry>
            {
                ["error"] = new HealthReportEntry(
                    HealthStatus.Degraded, 
                    "Health check service encountered an error but service remains operational", 
                    TimeSpan.Zero, 
                    ex, 
                    null)
            };

            return new HealthReport(entries, TimeSpan.Zero);
        }
    }
}