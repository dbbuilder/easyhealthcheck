// Copyright (c) EasyHealth. All rights reserved.
// Licensed under the MIT License.

namespace EasyHealth.HealthChecks.Checks
{
    using System;
    using System.Net;

    /// <summary>
    /// Options for HTTP health check configuration.
    /// </summary>
    public sealed class HttpHealthCheckOptions
    {
        /// <summary>
        /// Gets or sets the URL to check.
        /// </summary>
        public Uri? Url { get; set; }

        /// <summary>
        /// Gets or sets the timeout for HTTP requests.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets the expected HTTP status code.
        /// </summary>
        public HttpStatusCode ExpectedStatusCode { get; set; } = HttpStatusCode.OK;

        /// <summary>
        /// Gets or sets the name for this health check.
        /// </summary>
        public string Name { get; set; } = "HTTP Check";

        /// <summary>
        /// Gets or sets the response time threshold in milliseconds for degraded status.
        /// </summary>
        public long SlowResponseThresholdMs { get; set; } = 5000;
    }
}