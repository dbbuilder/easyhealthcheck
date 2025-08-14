// Copyright (c) EasyHealth. All rights reserved.
// Licensed under the MIT License.

namespace EasyHealth.HealthChecks.Checks
{
    using System;

    /// <summary>
    /// Options for memory health check configuration.
    /// </summary>
    public sealed class MemoryHealthCheckOptions
    {
        /// <summary>
        /// Gets or sets the maximum memory usage in MB before health check reports unhealthy.
        /// </summary>
        public int MaxMemoryMB { get; set; } = 1024;

        /// <summary>
        /// Gets or sets the warning threshold as a percentage of MaxMemoryMB.
        /// Default is 0.8 (80%).
        /// </summary>
        public double WarningThreshold { get; set; } = 0.8;
    }
}