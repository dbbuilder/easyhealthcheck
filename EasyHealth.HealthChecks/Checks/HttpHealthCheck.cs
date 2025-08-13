using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace EasyHealth.HealthChecks.Checks;

/// <summary>
/// Health check that verifies HTTP endpoint availability.
/// </summary>
public class HttpHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly Uri _uri;
    private readonly ILogger<HttpHealthCheck> _logger;

    public HttpHealthCheck(string url, ILogger<HttpHealthCheck> logger, HttpClient? httpClient = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        
        if (!Uri.TryCreate(url, UriKind.Absolute, out _uri!))
            throw new ArgumentException($"Invalid URL format: {url}", nameof(url));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using var response = await _httpClient.GetAsync(_uri, cancellationToken);
            stopwatch.Stop();

            var data = new Dictionary<string, object>
            {
                { "Url", _uri.ToString() },
                { "StatusCode", (int)response.StatusCode },
                { "ResponseTimeMs", stopwatch.ElapsedMilliseconds },
                { "IsSuccessStatusCode", response.IsSuccessStatusCode }
            };

            if (response.IsSuccessStatusCode)
            {
                var message = $"HTTP endpoint {_uri} responded successfully in {stopwatch.ElapsedMilliseconds}ms";
                return HealthCheckResult.Healthy(message, data);
            }
            else
            {
                var message = $"HTTP endpoint {_uri} returned status code {response.StatusCode}";
                _logger.LogWarning("HTTP health check failed: {Message}", message);
                return HealthCheckResult.Unhealthy(message, data: data);
            }
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
        {
            var message = $"HTTP endpoint {_uri} request was cancelled";
            _logger.LogWarning("HTTP health check cancelled: {Message}", message);
            return HealthCheckResult.Degraded(message, ex);
        }
        catch (TaskCanceledException ex)
        {
            var message = $"HTTP endpoint {_uri} request timed out";
            _logger.LogWarning("HTTP health check timed out: {Message}", message);
            return HealthCheckResult.Degraded(message, ex);
        }
        catch (HttpRequestException ex)
        {
            var message = $"HTTP endpoint {_uri} is unreachable";
            _logger.LogError(ex, "HTTP health check failed: {Message}", message);
            return HealthCheckResult.Unhealthy(message, ex);
        }
        catch (Exception ex)
        {
            var message = $"Unexpected error checking HTTP endpoint {_uri}";
            _logger.LogError(ex, "HTTP health check failed: {Message}", message);
            return HealthCheckResult.Degraded(message, ex);
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}