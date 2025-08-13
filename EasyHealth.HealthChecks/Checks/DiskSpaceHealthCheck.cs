using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace EasyHealth.HealthChecks.Checks;

/// <summary>
/// Options for disk space health check configuration.
/// </summary>
public class DiskSpaceHealthCheckOptions
{
    /// <summary>
    /// Minimum free disk space in GB before health check reports unhealthy.
    /// </summary>
    public int MinFreeSpaceGB { get; set; } = 1;

    /// <summary>
    /// Path to check disk space for. Defaults to current directory.
    /// </summary>
    public string Path { get; set; } = Environment.CurrentDirectory;
}

/// <summary>
/// Health check that monitors available disk space.
/// </summary>
public class DiskSpaceHealthCheck : IHealthCheck
{
    private readonly DiskSpaceHealthCheckOptions _options;
    private readonly ILogger<DiskSpaceHealthCheck> _logger;

    public DiskSpaceHealthCheck(DiskSpaceHealthCheckOptions options, ILogger<DiskSpaceHealthCheck> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var driveInfo = new DriveInfo(_options.Path);
            var freeSpaceGB = driveInfo.AvailableFreeSpace / 1024 / 1024 / 1024;
            var totalSpaceGB = driveInfo.TotalSize / 1024 / 1024 / 1024;
            var usedSpaceGB = totalSpaceGB - freeSpaceGB;
            var freeSpacePercentage = (double)freeSpaceGB / totalSpaceGB * 100;

            var data = new Dictionary<string, object>
            {
                { "FreeSpaceGB", freeSpaceGB },
                { "TotalSpaceGB", totalSpaceGB },
                { "UsedSpaceGB", usedSpaceGB },
                { "FreeSpacePercentage", Math.Round(freeSpacePercentage, 2) },
                { "MinRequiredGB", _options.MinFreeSpaceGB },
                { "DriveName", driveInfo.Name }
            };

            if (freeSpaceGB < _options.MinFreeSpaceGB)
            {
                var message = $"Free disk space {freeSpaceGB}GB is below minimum requirement of {_options.MinFreeSpaceGB}GB on drive {driveInfo.Name}";
                _logger.LogWarning("Disk space health check failed: {Message}", message);
                return Task.FromResult(HealthCheckResult.Unhealthy(message, data: data));
            }

            if (freeSpaceGB < _options.MinFreeSpaceGB * 2)
            {
                var message = $"Free disk space {freeSpaceGB}GB is getting low on drive {driveInfo.Name}";
                _logger.LogInformation("Disk space health check degraded: {Message}", message);
                return Task.FromResult(HealthCheckResult.Degraded(message, data: data));
            }

            var healthyMessage = $"Disk space {freeSpaceGB}GB ({freeSpacePercentage:F1}%) is sufficient on drive {driveInfo.Name}";
            return Task.FromResult(HealthCheckResult.Healthy(healthyMessage, data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check disk space for path: {Path}", _options.Path);
            return Task.FromResult(HealthCheckResult.Degraded($"Unable to check disk space for path: {_options.Path}", ex));
        }
    }
}