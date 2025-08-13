using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using EasyHealth.HealthChecks.Checks;

namespace EasyHealth.HealthChecks.Core;

/// <summary>
/// A wrapper health check that ensures resilient operation by never throwing exceptions
/// and always returning a meaningful result, even when the underlying health check fails.
/// </summary>
public class ResilientHealthCheck : IHealthCheck
{
    private readonly IHealthCheck _innerHealthCheck;
    private readonly ILogger<ResilientHealthCheck> _logger;

    public ResilientHealthCheck(IHealthCheck innerHealthCheck, ILogger<ResilientHealthCheck> logger)
    {
        _innerHealthCheck = innerHealthCheck ?? throw new ArgumentNullException(nameof(innerHealthCheck));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _innerHealthCheck.CheckHealthAsync(context, cancellationToken);
            return result;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Health check '{HealthCheckName}' timed out", context.Registration.Name);
            return HealthCheckResult.Degraded("Health check timed out but service remains operational", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check '{HealthCheckName}' failed with exception", context.Registration.Name);
            return HealthCheckResult.Degraded("Health check failed but service remains operational", ex);
        }
    }
}

/// <summary>
/// Configuration options for the EasyHealth health checks.
/// </summary>
public class EasyHealthConfiguration
{
    private readonly List<Action<IServiceCollection>> _configurators = new();

    internal IReadOnlyList<Action<IServiceCollection>> Configurators => _configurators;

    /// <summary>
    /// Add a database health check using the provided connection string.
    /// </summary>
    public EasyHealthConfiguration AddDatabase(string connectionString, string? name = null)
    {
        _configurators.Add(services =>
        {
            services.AddHealthChecks().AddSqlServer(connectionString, name: name ?? "database");
        });
        return this;
    }

    /// <summary>
    /// Add a memory usage health check.
    /// </summary>
    public EasyHealthConfiguration AddMemoryCheck(int maxMemoryMB = 1024, string? name = null)
    {
        _configurators.Add(services =>
        {
            services.AddSingleton(new MemoryHealthCheckOptions { MaxMemoryMB = maxMemoryMB });
            services.AddHealthChecks().AddCheck<MemoryHealthCheck>(name ?? "memory");
        });
        return this;
    }

    /// <summary>
    /// Add a disk space health check.
    /// </summary>
    public EasyHealthConfiguration AddDiskSpaceCheck(int minFreeSpaceGB = 1, string? path = null, string? name = null)
    {
        _configurators.Add(services =>
        {
            services.AddSingleton(new DiskSpaceHealthCheckOptions 
            { 
                MinFreeSpaceGB = minFreeSpaceGB,
                Path = path ?? Environment.CurrentDirectory
            });
            services.AddHealthChecks().AddCheck<DiskSpaceHealthCheck>(name ?? "disk_space");
        });
        return this;
    }

    /// <summary>
    /// Add an HTTP endpoint health check.
    /// </summary>
    public EasyHealthConfiguration AddHttpCheck(string url, string? name = null)
    {
        _configurators.Add(services =>
        {
            services.AddHealthChecks().AddUrlGroup(new Uri(url), name ?? "http_endpoint");
        });
        return this;
    }
}