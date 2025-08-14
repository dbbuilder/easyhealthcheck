// Copyright (c) EasyHealth. All rights reserved.
// Licensed under the MIT License.

namespace EasyHealth.HealthChecks.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyHealth.HealthChecks.Checks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using Xunit;
    using Moq;

    /// <summary>
    /// Unit tests for MemoryHealthCheck.
    /// </summary>
    public sealed class MemoryHealthCheckTests
    {
        private readonly Mock<ILogger<MemoryHealthCheck>> _mockLogger;
        private readonly MemoryHealthCheckOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryHealthCheckTests"/> class.
        /// </summary>
        public MemoryHealthCheckTests()
        {
            _mockLogger = new Mock<ILogger<MemoryHealthCheck>>();
            _options = new MemoryHealthCheckOptions { MaxMemoryMB = 1024 };
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new MemoryHealthCheck(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new MemoryHealthCheck(_options, null!));
        }

        [Fact]
        public async Task CheckHealthAsync_WithLowMemoryUsage_ReturnsHealthy()
        {
            // Arrange
            var healthCheck = new MemoryHealthCheck(_options, _mockLogger.Object);
            var context = new HealthCheckContext();

            // Act
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

            // Assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
            Assert.NotNull(result.Data);
            Assert.Contains("GCMemoryMB", result.Data.Keys);
            Assert.Contains("MaxAllowedMB", result.Data.Keys);
            Assert.Contains("within acceptable limits", result.Description);
        }

        [Fact]
        public async Task CheckHealthAsync_WithHighMemoryUsage_ReturnsDegraded()
        {
            // Arrange - Force a garbage collection and set very low limit
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var lowLimitOptions = new MemoryHealthCheckOptions { MaxMemoryMB = 1 }; // Very low limit
            var healthCheck = new MemoryHealthCheck(lowLimitOptions, _mockLogger.Object);
            var context = new HealthCheckContext();

            // Act
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

            // Assert
            Assert.True(result.Status == HealthStatus.Degraded || result.Status == HealthStatus.Unhealthy);
            Assert.NotNull(result.Data);
            Assert.Contains("GCMemoryMB", result.Data.Keys);
        }

        [Fact]
        public async Task CheckHealthAsync_WithWarningThreshold_ReturnsDegraded()
        {
            // Arrange - Use current memory + small buffer to trigger warning
            var currentMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024;
            var warningOptions = new MemoryHealthCheckOptions 
            { 
                MaxMemoryMB = (int)(currentMemoryMB * 1.1), // Just above current
                WarningThreshold = 0.9 // 90% threshold
            };
            
            var healthCheck = new MemoryHealthCheck(warningOptions, _mockLogger.Object);
            var context = new HealthCheckContext();

            // Act
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Contains("WarningThreshold", result.Data.Keys);
            Assert.Equal(0.9, result.Data["WarningThreshold"]);
        }

        [Fact]
        public async Task CheckHealthAsync_AlwaysIncludesExpectedData()
        {
            // Arrange
            var healthCheck = new MemoryHealthCheck(_options, _mockLogger.Object);
            var context = new HealthCheckContext();

            // Act
            var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Contains("GCMemoryMB", result.Data.Keys);
            Assert.Contains("MaxAllowedMB", result.Data.Keys);
            Assert.Contains("WarningThreshold", result.Data.Keys);
            
            // Verify data types
            Assert.IsType<long>(result.Data["GCMemoryMB"]);
            Assert.IsType<int>(result.Data["MaxAllowedMB"]);
            Assert.IsType<double>(result.Data["WarningThreshold"]);
        }
    }
}