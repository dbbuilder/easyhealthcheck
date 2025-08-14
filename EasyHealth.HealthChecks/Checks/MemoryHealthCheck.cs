// Copyright (c) EasyHealth. All rights reserved.
// Licensed under the MIT License.

namespace EasyHealth.HealthChecks.Checks
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Health check that monitors memory usage of the current process.
    /// </summary>
    public sealed class MemoryHealthCheck : IHealthCheck
    {
        private static readonly Action<ILogger, string, Exception?> LogWarningAction =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1, "MemoryCheckFailed"), "Memory health check failed: {Message}");

        private static readonly Action<ILogger, string, Exception?> LogInfoAction =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(2, "MemoryCheckDegraded"), "Memory health check degraded: {Message}");

        private static readonly Action<ILogger, Exception?> LogErrorAction =
            LoggerMessage.Define(LogLevel.Error, new EventId(3, "MemoryCheckError"), "Failed to check memory usage");

        private readonly MemoryHealthCheckOptions _options;
        private readonly ILogger<MemoryHealthCheck> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryHealthCheck"/> class.
        /// </summary>
        /// <param name="options">The memory health check options.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when options or logger is null.</exception>
        public MemoryHealthCheck(MemoryHealthCheckOptions options, ILogger<MemoryHealthCheck> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Use GC memory instead of Process.WorkingSet64 for better security and accuracy
                var gcMemoryBytes = GC.GetTotalMemory(forceFullCollection: false);
                var gcMemoryMB = gcMemoryBytes / 1024 / 1024;

                var data = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    { "GCMemoryMB", gcMemoryMB },
                    { "MaxAllowedMB", _options.MaxMemoryMB },
                    { "WarningThreshold", _options.WarningThreshold }
                };

                if (gcMemoryMB > _options.MaxMemoryMB)
                {
                    var message = $"Memory usage {gcMemoryMB}MB exceeds limit of {_options.MaxMemoryMB}MB";
                    LogWarningAction(_logger, message, null);
                    return Task.FromResult(HealthCheckResult.Unhealthy(message, data: data));
                }

                var warningThreshold = _options.MaxMemoryMB * _options.WarningThreshold;
                if (gcMemoryMB > warningThreshold)
                {
                    var message = string.Format(
                        CultureInfo.InvariantCulture,
                        "Memory usage {0}MB is approaching limit of {1}MB",
                        gcMemoryMB,
                        _options.MaxMemoryMB);
                    LogInfoAction(_logger, message, null);
                    return Task.FromResult(HealthCheckResult.Degraded(message, data: data));
                }

                var healthyMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Memory usage {0}MB is within acceptable limits",
                    gcMemoryMB);
                return Task.FromResult(HealthCheckResult.Healthy(healthyMessage, data));
            }
            catch (OutOfMemoryException ex)
            {
                LogErrorAction(_logger, ex);
                return Task.FromResult(HealthCheckResult.Unhealthy("Out of memory", ex));
            }
            catch (InvalidOperationException ex)
            {
                LogErrorAction(_logger, ex);
                return Task.FromResult(HealthCheckResult.Degraded("Unable to check memory usage", ex));
            }
        }
    }
}