// Copyright (c) EasyHealth. All rights reserved.
// Licensed under the MIT License.

namespace EasyHealth.HealthChecks.Checks
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Health check that monitors available disk space.
    /// </summary>
    public sealed class DiskSpaceHealthCheck : IHealthCheck
    {
        private static readonly Action<ILogger, string, Exception?> LogWarningAction =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1, "DiskSpaceCheckFailed"), "Disk space health check failed: {Message}");

        private static readonly Action<ILogger, string, Exception?> LogInfoAction =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(2, "DiskSpaceCheckDegraded"), "Disk space health check degraded: {Message}");

        private static readonly Action<ILogger, string, Exception?> LogErrorAction =
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(3, "DiskSpaceCheckError"), "Failed to check disk space for path: {Path}");

        private readonly DiskSpaceHealthCheckOptions _options;
        private readonly ILogger<DiskSpaceHealthCheck> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskSpaceHealthCheck"/> class.
        /// </summary>
        /// <param name="options">The disk space health check options.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when options or logger is null.</exception>
        public DiskSpaceHealthCheck(DiskSpaceHealthCheckOptions options, ILogger<DiskSpaceHealthCheck> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var driveInfo = new DriveInfo(_options.Path);
                
                // Check if drive is ready before accessing properties
                if (!driveInfo.IsReady)
                {
                    var notReadyMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Drive {0} is not ready",
                        driveInfo.Name);
                    return Task.FromResult(HealthCheckResult.Degraded(notReadyMessage));
                }

                var freeSpaceGB = driveInfo.AvailableFreeSpace / 1024 / 1024 / 1024;
                var totalSpaceGB = driveInfo.TotalSize / 1024 / 1024 / 1024;
                var usedSpaceGB = totalSpaceGB - freeSpaceGB;
                var freeSpacePercentage = totalSpaceGB > 0 ? (double)freeSpaceGB / totalSpaceGB * 100 : 0;

                var data = new Dictionary<string, object>(StringComparer.Ordinal)
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
                    var message = string.Format(
                        CultureInfo.InvariantCulture,
                        "Free disk space {0}GB is below minimum requirement of {1}GB on drive {2}",
                        freeSpaceGB,
                        _options.MinFreeSpaceGB,
                        driveInfo.Name);
                    LogWarningAction(_logger, message, null);
                    return Task.FromResult(HealthCheckResult.Unhealthy(message, data: data));
                }

                var warningThreshold = _options.MinFreeSpaceGB * _options.WarningThresholdMultiplier;
                if (freeSpaceGB < warningThreshold)
                {
                    var message = string.Format(
                        CultureInfo.InvariantCulture,
                        "Free disk space {0}GB is getting low on drive {1}",
                        freeSpaceGB,
                        driveInfo.Name);
                    LogInfoAction(_logger, message, null);
                    return Task.FromResult(HealthCheckResult.Degraded(message, data: data));
                }

                var healthyMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Disk space {0}GB ({1:F1}%) is sufficient on drive {2}",
                    freeSpaceGB,
                    freeSpacePercentage,
                    driveInfo.Name);
                return Task.FromResult(HealthCheckResult.Healthy(healthyMessage, data));
            }
            catch (ArgumentException ex)
            {
                LogErrorAction(_logger, _options.Path, ex);
                return Task.FromResult(HealthCheckResult.Degraded($"Invalid path: {_options.Path}", ex));
            }
            catch (DirectoryNotFoundException ex)
            {
                LogErrorAction(_logger, _options.Path, ex);
                return Task.FromResult(HealthCheckResult.Degraded($"Directory not found: {_options.Path}", ex));
            }
            catch (DriveNotFoundException ex)
            {
                LogErrorAction(_logger, _options.Path, ex);
                return Task.FromResult(HealthCheckResult.Degraded($"Drive not found for path: {_options.Path}", ex));
            }
            catch (UnauthorizedAccessException ex)
            {
                LogErrorAction(_logger, _options.Path, ex);
                return Task.FromResult(HealthCheckResult.Degraded($"Access denied to path: {_options.Path}", ex));
            }
        }
    }
}