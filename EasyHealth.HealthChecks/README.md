# EasyHealth.HealthChecks

A comprehensive health check package for ASP.NET Core applications with minimal configuration and maximum reliability. Always returns results even when the application has issues.

## Features

- 🚀 **Minimal Configuration**: Add health checks with just one line of code
- 🛡️ **Always Reliable**: Never throws exceptions, always returns meaningful results
- 📊 **Built-in Checks**: Memory, disk space, database, HTTP endpoints
- 🎯 **Multi-targeting**: Supports .NET 8 and .NET 9
- 📈 **Detailed Reporting**: Rich JSON responses with timing and metadata
- 🔧 **Extensible**: Easy to add custom health checks

## Quick Start

### 1. Install the Package

```bash
dotnet add package EasyHealth.HealthChecks
```

### 2. Add to Your Program.cs

```csharp
using EasyHealth.HealthChecks.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEasyHealthChecks();

var app = builder.Build();

// Add middleware
app.UseEasyHealthChecks();

app.Run();
```

That's it! Your application now has health checks at `/health`.

## Configuration Examples

### Basic with Database

```csharp
builder.Services.AddEasyHealthChecks("Server=localhost;Database=MyApp;Integrated Security=true;");
```

### Advanced Configuration

```csharp
builder.Services.AddEasyHealthChecks(config =>
{
    config.AddDatabase("Server=localhost;Database=MyApp;Integrated Security=true;");
    config.AddMemoryCheck(maxMemoryMB: 2048);
    config.AddDiskSpaceCheck(minFreeSpaceGB: 10);
    config.AddHttpCheck("https://api.external-service.com/health");
});
```

### Multiple Endpoints

```csharp
// Basic health check
app.UseEasyHealthChecks("/health");

// Detailed health check for monitoring systems
app.UseDetailedEasyHealthChecks("/health/detailed");
```

## Built-in Health Checks

### Memory Check
Monitors application memory usage:
- **Healthy**: Memory usage below 80% of limit
- **Degraded**: Memory usage between 80% and 100% of limit
- **Unhealthy**: Memory usage exceeds limit

### Disk Space Check
Monitors available disk space:
- **Healthy**: Free space above 2x minimum requirement
- **Degraded**: Free space between 1x and 2x minimum requirement
- **Unhealthy**: Free space below minimum requirement

### Database Check
Verifies database connectivity using Entity Framework health checks.

### HTTP Endpoint Check
Monitors external service availability with configurable timeouts.

## Health Check Responses

### Basic Response (`/health`)
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "totalDuration": 125.5,
  "checks": [
    {
      "name": "memory",
      "status": "Healthy",
      "description": "Memory usage 512MB is within acceptable limits",
      "duration": 2.1,
      "data": {
        "workingSetMB": 512,
        "gcMemoryMB": 256,
        "maxAllowedMB": 1024
      }
    }
  ]
}
```

### Detailed Response (`/health/detailed`)
Includes additional system information like environment, machine name, and process ID.

## Resilient Design

EasyHealth.HealthChecks is designed to never fail catastrophically:

- **No Exceptions**: All health checks are wrapped to prevent exceptions from bubbling up
- **Timeouts**: Automatic timeouts prevent hanging health checks
- **Graceful Degradation**: When checks fail, the service reports "Degraded" instead of "Unhealthy"
- **Always Returns**: Even if the entire health check system fails, a meaningful response is returned

## Best Practices

1. **Use in Production**: The resilient design makes it safe for production monitoring
2. **Monitor Trends**: Use the detailed endpoint for monitoring systems that can track trends
3. **Set Appropriate Limits**: Configure memory and disk space limits based on your application's needs
4. **External Dependencies**: Use HTTP checks for critical external services
5. **Database Monitoring**: Always include database health checks for data-driven applications

## License

MIT License - see LICENSE file for details.