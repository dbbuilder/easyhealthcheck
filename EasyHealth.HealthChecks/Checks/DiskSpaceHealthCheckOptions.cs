// Copyright (c) EasyHealth. All rights reserved.
// Licensed under the MIT License.

namespace EasyHealth.HealthChecks.Checks
{
    using System;

    /// <summary>
    /// Options for disk space health check configuration.
    /// </summary>
    public sealed class DiskSpaceHealthCheckOptions
    {
        /// <summary>
        /// Gets or sets the minimum free disk space in GB before health check reports unhealthy.
        /// </summary>
        public int MinFreeSpaceGB { get; set; } = 1;

        /// <summary>
        /// Gets or sets the path to check disk space for. Defaults to current directory.
        /// </summary>
        public string Path { get; set; } = Environment.CurrentDirectory;

        /// <summary>
        /// Gets or sets the warning threshold multiplier for degraded status.
        /// Default is 2.0 (warn when free space is less than 2x minimum).
        /// </summary>
        public double WarningThresholdMultiplier { get; set; } = 2.0;
    }
}