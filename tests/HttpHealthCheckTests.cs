// Copyright (c) EasyHealth. All rights reserved.
// Licensed under the MIT License.

namespace EasyHealth.HealthChecks.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyHealth.HealthChecks.Checks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using Xunit;
    using Moq;
    using Moq.Protected;

    /// <summary>
    /// Unit tests for HttpHealthCheck.
    /// </summary>
    public sealed class HttpHealthCheckTests : IDisposable
    {
        private readonly Mock<ILogger<HttpHealthCheck>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly HttpHealthCheckOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHealthCheckTests"/> class.
        /// </summary>
        public HttpHealthCheckTests()
        {
            _mockLogger = new Mock<ILogger<HttpHealthCheck>>();
            _mockHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHandler.Object);
            _options = new HttpHealthCheckOptions 
            { 
                Url = new Uri("https://example.com/health"),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new HttpHealthCheck(null!, _mockLogger.Object, _httpClient));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new HttpHealthCheck(_options, null!, _httpClient));
        }

        [Fact]
        public void Constructor_WithNullUrl_ThrowsArgumentException()
        {
            // Arrange
            var invalidOptions = new HttpHealthCheckOptions { Url = null };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                new HttpHealthCheck(invalidOptions, _mockLogger.Object, _httpClient));
        }

        [Fact]
        public async Task CheckHealthAsync_WithSuccessfulResponse_ReturnsHealthy()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            SetupHttpResponse(response);

            var healthCheck = new HttpHealthCheck(_options, _mockLogger.Object, _httpClient);
            var context = new HealthCheckContext();

            // Act
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

            // Assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
            Assert.NotNull(result.Data);
            Assert.Contains("Url", result.Data.Keys);
            Assert.Contains("StatusCode", result.Data.Keys);
            Assert.Contains("ResponseTimeMs", result.Data.Keys);
            Assert.Contains("responded successfully", result.Description);
        }

        [Fact]
        public async Task CheckHealthAsync_WithSlowResponse_ReturnsDegraded()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var slowOptions = new HttpHealthCheckOptions 
            { 
                Url = new Uri("https://example.com/health"),
                SlowResponseThresholdMs = 1 // Very low threshold
            };

            SetupHttpResponse(response, TimeSpan.FromMilliseconds(100)); // Simulate slow response

            var healthCheck = new HttpHealthCheck(slowOptions, _mockLogger.Object, _httpClient);
            var context = new HealthCheckContext();

            // Act
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

            // Assert
            Assert.Equal(HealthStatus.Degraded, result.Status);
            Assert.Contains("responded slowly", result.Description);
        }

        [Fact]
        public async Task CheckHealthAsync_WithUnexpectedStatusCode_ReturnsUnhealthy()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            SetupHttpResponse(response);

            var healthCheck = new HttpHealthCheck(_options, _mockLogger.Object, _httpClient);
            var context = new HealthCheckContext();

            // Act
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

            // Assert
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.Contains("returned status code", result.Description);
            Assert.Contains("InternalServerError", result.Description);
        }

        [Fact]
        public async Task CheckHealthAsync_WithCustomExpectedStatusCode_ReturnsHealthy()
        {
            // Arrange
            var customOptions = new HttpHealthCheckOptions 
            { 
                Url = new Uri("https://example.com/health"),
                ExpectedStatusCode = HttpStatusCode.Accepted
            };

            var response = new HttpResponseMessage(HttpStatusCode.Accepted);
            SetupHttpResponse(response);

            var healthCheck = new HttpHealthCheck(customOptions, _mockLogger.Object, _httpClient);
            var context = new HealthCheckContext();

            // Act
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

            // Assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
            Assert.Equal(202, result.Data["StatusCode"]); // Accepted = 202
            Assert.Equal(202, result.Data["ExpectedStatusCode"]);
        }

        [Fact]
        public async Task CheckHealthAsync_WithHttpRequestException_ReturnsUnhealthy()
        {
            // Arrange
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection failed"));

            var healthCheck = new HttpHealthCheck(_options, _mockLogger.Object, _httpClient);
            var context = new HealthCheckContext();

            // Act
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

            // Assert
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.Contains("unreachable", result.Description);
        }

        [Fact]
        public async Task CheckHealthAsync_WithTimeout_ReturnsDegraded()
        {
            // Arrange
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("The operation was canceled."));

            var healthCheck = new HttpHealthCheck(_options, _mockLogger.Object, _httpClient);
            var context = new HealthCheckContext();

            // Act
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

            // Assert
            Assert.Equal(HealthStatus.Degraded, result.Status);
            Assert.Contains("timed out", result.Description);
        }

        [Fact]
        public async Task CheckHealthAsync_AlwaysIncludesExpectedData()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            SetupHttpResponse(response);

            var healthCheck = new HttpHealthCheck(_options, _mockLogger.Object, _httpClient);
            var context = new HealthCheckContext();

            // Act
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Contains("Url", result.Data.Keys);
            Assert.Contains("StatusCode", result.Data.Keys);
            Assert.Contains("ResponseTimeMs", result.Data.Keys);
            Assert.Contains("IsSuccessStatusCode", result.Data.Keys);
            Assert.Contains("ExpectedStatusCode", result.Data.Keys);
            
            // Verify data types and values
            Assert.Equal(_options.Url.ToString(), result.Data["Url"]);
            Assert.IsType<int>(result.Data["StatusCode"]);
            Assert.IsType<long>(result.Data["ResponseTimeMs"]);
            Assert.IsType<bool>(result.Data["IsSuccessStatusCode"]);
            Assert.IsType<int>(result.Data["ExpectedStatusCode"]);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private void SetupHttpResponse(HttpResponseMessage response, TimeSpan? delay = null)
        {
            var setupResult = _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());

            if (delay.HasValue)
            {
                setupResult.Returns(async () =>
                {
                    await Task.Delay(delay.Value);
                    return response;
                });
            }
            else
            {
                setupResult.ReturnsAsync(response);
            }
        }
    }
}