using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace EasyHealth.HealthChecks.Checks;

/// <summary>
/// Options for memory health check configuration.
/// </summary>
public class MemoryHealthCheckOptions
{
    /// <summary>
    /// Maximum memory usage in MB before health check reports unhealthy.
    /// </summary>
    public int MaxMemoryMB { get; set; } = 1024;
}

/// <summary>
/// Health check that monitors memory usage of the current process.
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly MemoryHealthCheckOptions _options;
    private readonly ILogger<MemoryHealthCheck> _logger;

    public MemoryHealthCheck(MemoryHealthCheckOptions options, ILogger<MemoryHealthCheck> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var memoryUsedMB = process.WorkingSet64 / 1024 / 1024;
            var gcMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024;

            var data = new Dictionary<string, object>
            {
                { "WorkingSetMB", memoryUsedMB },
                { "GCMemoryMB", gcMemoryMB },
                { "MaxAllowedMB", _options.MaxMemoryMB }
            };

            if (memoryUsedMB > _options.MaxMemoryMB)
            {
                var message = $"Memory usage {memoryUsedMB}MB exceeds limit of {_options.MaxMemoryMB}MB";
                _logger.LogWarning("Memory health check failed: {Message}", message);
                return Task.FromResult(HealthCheckResult.Unhealthy(message, data: data));
            }

            if (memoryUsedMB > _options.MaxMemoryMB * 0.8)
            {
                var message = $"Memory usage {memoryUsedMB}MB is approaching limit of {_options.MaxMemoryMB}MB";
                _logger.LogInformation("Memory health check degraded: {Message}", message);
                return Task.FromResult(HealthCheckResult.Degraded(message, data: data));
            }

            var healthyMessage = $"Memory usage {memoryUsedMB}MB is within acceptable limits";
            return Task.FromResult(HealthCheckResult.Healthy(healthyMessage, data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check memory usage");
            return Task.FromResult(HealthCheckResult.Degraded("Unable to check memory usage", ex));
        }
    }
}