// Copyright (c) EasyHealth. All rights reserved.
// Licensed under the MIT License.

namespace EasyHealth.HealthChecks.Checks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Health check that verifies HTTP endpoint availability.
    /// </summary>
    public sealed class HttpHealthCheck : IHealthCheck, IDisposable
    {
        private static readonly Action<ILogger, string, Exception?> LogWarningAction =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1, "HttpCheckFailed"), "HTTP health check failed: {Message}");

        private static readonly Action<ILogger, string, Exception?> LogInfoAction =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(2, "HttpCheckSlow"), "HTTP health check slow response: {Message}");

        private static readonly Action<ILogger, string, Exception?> LogErrorAction =
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(3, "HttpCheckError"), "HTTP health check error: {Message}");

        private readonly HttpClient _httpClient;
        private readonly Uri _uri;
        private readonly HttpHealthCheckOptions _options;
        private readonly ILogger<HttpHealthCheck> _logger;
        private readonly bool _disposeHttpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHealthCheck"/> class.
        /// </summary>
        /// <param name="options">The HTTP health check options.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="httpClient">Optional HTTP client instance. If null, a new one will be created.</param>
        /// <exception cref="ArgumentNullException">Thrown when options or logger is null.</exception>
        /// <exception cref="ArgumentException">Thrown when URL is invalid.</exception>
        public HttpHealthCheck(HttpHealthCheckOptions options, ILogger<HttpHealthCheck> logger, HttpClient? httpClient = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (_options.Url == null)
            {
                throw new ArgumentException("URL cannot be null", nameof(options));
            }

            _uri = _options.Url;

            if (httpClient != null)
            {
                _httpClient = httpClient;
                _disposeHttpClient = false;
            }
            else
            {
                _httpClient = new HttpClient { Timeout = _options.Timeout };
                _disposeHttpClient = true;
            }
        }

        /// <inheritdoc/>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                using var response = await _httpClient.GetAsync(_uri, cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();

                var data = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    { "Url", _uri.ToString() },
                    { "StatusCode", (int)response.StatusCode },
                    { "ResponseTimeMs", stopwatch.ElapsedMilliseconds },
                    { "IsSuccessStatusCode", response.IsSuccessStatusCode },
                    { "ExpectedStatusCode", (int)_options.ExpectedStatusCode }
                };

                // Check if response matches expected status code
                if (response.StatusCode == _options.ExpectedStatusCode)
                {
                    // Check for slow response
                    if (stopwatch.ElapsedMilliseconds > _options.SlowResponseThresholdMs)
                    {
                        var slowMessage = string.Format(
                            CultureInfo.InvariantCulture,
                            "HTTP endpoint {0} responded slowly in {1}ms (threshold: {2}ms)",
                            _uri,
                            stopwatch.ElapsedMilliseconds,
                            _options.SlowResponseThresholdMs);
                        LogInfoAction(_logger, slowMessage, null);
                        return HealthCheckResult.Degraded(slowMessage, null, data);
                    }

                    var successMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "HTTP endpoint {0} responded successfully in {1}ms",
                        _uri,
                        stopwatch.ElapsedMilliseconds);
                    return HealthCheckResult.Healthy(successMessage, data);
                }
                else
                {
                    var failureMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "HTTP endpoint {0} returned status code {1}, expected {2}",
                        _uri,
                        response.StatusCode,
                        _options.ExpectedStatusCode);
                    LogWarningAction(_logger, failureMessage, null);
                    return HealthCheckResult.Unhealthy(failureMessage, data: data);
                }
            }
            catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested && cancellationToken.IsCancellationRequested)
            {
                var cancelMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "HTTP endpoint {0} request was cancelled",
                    _uri);
                LogWarningAction(_logger, cancelMessage, ex);
                return HealthCheckResult.Degraded(cancelMessage, ex);
            }
            catch (TaskCanceledException ex)
            {
                var timeoutMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "HTTP endpoint {0} request timed out after {1}ms",
                    _uri,
                    _options.Timeout.TotalMilliseconds);
                LogWarningAction(_logger, timeoutMessage, ex);
                return HealthCheckResult.Degraded(timeoutMessage, ex);
            }
            catch (HttpRequestException ex)
            {
                var unreachableMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "HTTP endpoint {0} is unreachable",
                    _uri);
                LogErrorAction(_logger, unreachableMessage, ex);
                return HealthCheckResult.Unhealthy(unreachableMessage, ex);
            }
            catch (InvalidOperationException ex)
            {
                var invalidOpMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Invalid operation when checking HTTP endpoint {0}",
                    _uri);
                LogErrorAction(_logger, invalidOpMessage, ex);
                return HealthCheckResult.Degraded(invalidOpMessage, ex);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposeHttpClient)
            {
                _httpClient?.Dispose();
            }
        }
    }
}