# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-01-15

### Added
- Initial release of EasyHealth.HealthChecks
- Multi-targeting support for .NET 8 and .NET 9
- Built-in health checks:
  - Memory usage monitoring
  - Disk space monitoring  
  - HTTP endpoint monitoring
  - Database connectivity (via AspNetCore.HealthChecks.SqlServer)
- Resilient health check infrastructure:
  - `ResilientHealthCheck` wrapper for individual checks
  - `ResilientHealthCheckService` for service-level resilience
  - Automatic timeouts and graceful degradation
- Simple configuration API:
  - `AddEasyHealthChecks()` for default setup
  - `AddEasyHealthChecks(connectionString)` for database monitoring
  - `AddEasyHealthChecks(config => {...})` for advanced configuration
- Rich JSON response formats:
  - Basic endpoint at `/health`
  - Detailed endpoint at `/health/detailed`
- Comprehensive documentation and examples
- MIT license
- GitHub Actions CI/CD pipeline
- NuGet package with symbols

### Features
- **Zero-exception guarantee**: Health checks never throw unhandled exceptions
- **Always responsive**: Returns meaningful results even when checks fail
- **Configurable thresholds**: Customizable limits for memory and disk space
- **Detailed metrics**: Rich data in health check responses
- **Production-ready**: Designed for high-availability production environments

### Dependencies
- Microsoft.Extensions.Diagnostics.HealthChecks (8.0.0)
- Microsoft.Extensions.Hosting.Abstractions (8.0.0)
- AspNetCore.HealthChecks.SqlServer (8.0.2)
- AspNetCore.HealthChecks.Uris (8.0.1)
- Scrutor (4.2.2)

## [Unreleased]

### Planned
- Redis health check support
- Custom metrics collection
- Performance improvements
- Additional database providers
- Kubernetes health check integration