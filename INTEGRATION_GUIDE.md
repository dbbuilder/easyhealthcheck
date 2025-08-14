# EasyHealth.HealthChecks Integration Guide

## Overview

EasyHealth.HealthChecks is an enterprise-grade NuGet package that provides comprehensive health monitoring for .NET applications. This guide will help you integrate the health checks into your application quickly and effectively.

## Prerequisites

- .NET 8.0 or .NET 9.0
- ASP.NET Core application
- Visual Studio 2022 or VS Code with C# Dev Kit

## Installation

### Package Manager Console
```powershell
Install-Package EasyHealth.HealthChecks -Version 1.0.2
```

### .NET CLI
```bash
dotnet add package EasyHealth.HealthChecks --version 1.0.2
```

### PackageReference (in .csproj)
```xml
<PackageReference Include="EasyHealth.HealthChecks" Version="1.0.2" />
```

## Quick Start

### 1. Basic Setup

Add health checks to your `Program.cs`:

```csharp
using EasyHealth.HealthChecks.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add EasyHealth health checks
builder.Services.AddEasyHealthChecks();

var app = builder.Build();

// Map health check endpoints
app.MapEasyHealthChecks();

app.Run();
```

### 2. Access Health Check Endpoints

After setup, your application will expose these endpoints:

- **`/health`** - Overall health status
- **`/health/ready`** - Readiness probe (Kubernetes-compatible)
- **`/health/live`** - Liveness probe (Kubernetes-compatible)

### 3. Sample Response

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0156243",
  "entries": {
    "memory_check": {
      "status": "Healthy",
      "description": "Memory usage is within acceptable limits",
      "duration": "00:00:00.0001234",
      "data": {
        "AllocatedBytes": 45678901,
        "WorkingSetBytes": 123456789,
        "MemoryThresholdBytes": 1073741824,
        "MemoryUsagePercentage": 11.5
      }
    }
  }
}
```

## Available Health Checks

### Memory Health Check

Monitors system memory usage and alerts when thresholds are exceeded.

```csharp
builder.Services.AddEasyHealthChecks(options =>
{
    options.Memory.ThresholdBytes = 1_073_741_824; // 1GB
    options.Memory.DegradedThresholdBytes = 858_993_459; // 800MB
});
```

**Configuration Options:**
- `ThresholdBytes` - Maximum memory before unhealthy (default: 1GB)
- `DegradedThresholdBytes` - Memory limit before degraded status (default: 80% of threshold)

### HTTP Health Check

Monitors external HTTP endpoints for availability and response time.

```csharp
builder.Services.AddEasyHealthChecks(options =>
{
    options.Http.Add(new HttpHealthCheckOptions
    {
        Name = "external_api",
        Url = new Uri("https://api.example.com/health"),
        Timeout = TimeSpan.FromSeconds(30),
        ExpectedStatusCode = HttpStatusCode.OK,
        SlowResponseThresholdMs = 2000
    });
});
```

**Configuration Options:**
- `Name` - Unique identifier for the check
- `Url` - Target endpoint URL
- `Timeout` - Request timeout (default: 30 seconds)
- `ExpectedStatusCode` - Expected HTTP status (default: 200 OK)
- `SlowResponseThresholdMs` - Threshold for degraded status (default: 5000ms)

## Advanced Configuration

### Custom Configuration

```csharp
builder.Services.AddEasyHealthChecks(options =>
{
    // Memory configuration
    options.Memory.ThresholdBytes = 2_147_483_648; // 2GB
    options.Memory.DegradedThresholdBytes = 1_717_986_918; // 1.6GB
    
    // Multiple HTTP endpoints
    options.Http.AddRange(new[]
    {
        new HttpHealthCheckOptions
        {
            Name = "user_service",
            Url = new Uri("https://users.api.company.com/health")
        },
        new HttpHealthCheckOptions
        {
            Name = "payment_service", 
            Url = new Uri("https://payments.api.company.com/health"),
            ExpectedStatusCode = HttpStatusCode.Accepted,
            SlowResponseThresholdMs = 1000
        }
    });
});
```

### Custom Health Check Tags

```csharp
builder.Services.AddEasyHealthChecks(options =>
{
    options.Memory.Tags = new[] { "memory", "system", "critical" };
    
    options.Http.Add(new HttpHealthCheckOptions
    {
        Name = "database_api",
        Url = new Uri("https://db.api.company.com/health"),
        Tags = new[] { "database", "external", "critical" }
    });
});
```

### Environment-Specific Configuration

```csharp
builder.Services.AddEasyHealthChecks(options =>
{
    if (builder.Environment.IsProduction())
    {
        options.Memory.ThresholdBytes = 4_294_967_296; // 4GB in production
        options.Http.Add(new HttpHealthCheckOptions
        {
            Name = "prod_api",
            Url = new Uri("https://prod.api.company.com/health")
        });
    }
    else
    {
        options.Memory.ThresholdBytes = 1_073_741_824; // 1GB in dev/test
        options.Http.Add(new HttpHealthCheckOptions  
        {
            Name = "dev_api",
            Url = new Uri("https://dev.api.company.com/health")
        });
    }
});
```

## Integration Patterns

### 1. Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp
spec:
  template:
    spec:
      containers:
      - name: myapp
        image: myapp:latest
        ports:
        - containerPort: 8080
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

### 2. Docker Health Check

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MyApp.dll"]
```

### 3. Application Insights Integration

```csharp
builder.Services.AddEasyHealthChecks()
    .AddApplicationInsightsPublisher();

// Custom telemetry for health check results
builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(10);
    options.Period = TimeSpan.FromSeconds(30);
});
```

### 4. Custom Endpoint Configuration

```csharp
app.MapEasyHealthChecks(options =>
{
    options.HealthEndpoint = "/api/health";
    options.ReadyEndpoint = "/api/health/ready";
    options.LiveEndpoint = "/api/health/live";
});
```

## Security Considerations

### 1. Restrict Access in Production

```csharp
app.MapEasyHealthChecks()
   .RequireAuthorization("HealthCheckPolicy");

// Configure authorization policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HealthCheckPolicy", policy =>
        policy.RequireRole("Administrator", "HealthMonitor"));
});
```

### 2. Network-Level Protection

```csharp
app.MapEasyHealthChecks()
   .RequireHost("internal.company.com", "localhost");
```

### 3. API Key Authentication

```csharp
app.MapEasyHealthChecks()
   .AddEndpointFilter(async (context, next) =>
   {
       if (!context.HttpContext.Request.Headers.TryGetValue("X-API-Key", out var apiKey) ||
           apiKey != "your-secret-api-key")
       {
           context.HttpContext.Response.StatusCode = 401;
           return Results.Unauthorized();
       }
       return await next(context);
   });
```

## Monitoring and Alerting

### 1. Health Check Dashboard

Create a simple dashboard to monitor health status:

```csharp
app.MapGet("/health/dashboard", async (IServiceProvider services) =>
{
    var healthCheckService = services.GetRequiredService<HealthCheckService>();
    var result = await healthCheckService.CheckHealthAsync();
    
    return Results.Content($"""
        <html>
        <head><title>Health Dashboard</title></head>
        <body>
            <h1>Application Health Status</h1>
            <p>Overall Status: <strong>{result.Status}</strong></p>
            <p>Total Duration: {result.TotalDuration}</p>
            <ul>
            {string.Join("", result.Entries.Select(entry => 
                $"<li>{entry.Key}: {entry.Value.Status} ({entry.Value.Duration})</li>"))}
            </ul>
        </body>
        </html>
        """, "text/html");
});
```

### 2. Custom Alerting

```csharp
builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.Zero;
    options.Period = TimeSpan.FromMinutes(1);
});

builder.Services.AddSingleton<IHealthCheckPublisher, AlertingHealthCheckPublisher>();

public class AlertingHealthCheckPublisher : IHealthCheckPublisher
{
    private readonly ILogger<AlertingHealthCheckPublisher> _logger;
    
    public AlertingHealthCheckPublisher(ILogger<AlertingHealthCheckPublisher> logger)
    {
        _logger = logger;
    }
    
    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        if (report.Status == HealthStatus.Unhealthy)
        {
            _logger.LogCritical("Application is unhealthy: {Entries}", 
                string.Join(", ", report.Entries.Where(e => e.Value.Status == HealthStatus.Unhealthy)
                    .Select(e => e.Key)));
            
            // Send alert to monitoring system
            // await SendAlert(report);
        }
        
        return Task.CompletedTask;
    }
}
```

## Troubleshooting

### Common Issues

#### Health Check Not Appearing
**Problem:** Health check endpoints return 404
**Solution:** Ensure `app.MapEasyHealthChecks()` is called after `var app = builder.Build()`

#### Memory Check Always Unhealthy  
**Problem:** Memory threshold too low for your application
**Solution:** Adjust the threshold in configuration:
```csharp
options.Memory.ThresholdBytes = 2_147_483_648; // Increase to 2GB
```

#### HTTP Check Timeouts
**Problem:** External services taking too long to respond
**Solution:** Increase timeout or adjust thresholds:
```csharp
options.Http.Add(new HttpHealthCheckOptions
{
    Timeout = TimeSpan.FromMinutes(1),
    SlowResponseThresholdMs = 10000
});
```

### Logging

Enable detailed logging for troubleshooting:

```csharp
builder.Logging.AddFilter("EasyHealth.HealthChecks", LogLevel.Debug);
```

### Performance Considerations

- Health checks run synchronously on each request
- For high-traffic applications, consider caching results:

```csharp
builder.Services.Configure<HealthCheckOptions>(options =>
{
    options.ResultStatusCodes[HealthStatus.Healthy] = StatusCodes.Status200OK;
    options.ResultStatusCodes[HealthStatus.Degraded] = StatusCodes.Status200OK;
    options.ResultStatusCodes[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable;
});
```

## Examples Repository

Complete example applications are available at:
- [Basic Web API Example](examples/BasicWebApi/)
- [Kubernetes Integration Example](examples/KubernetesApp/)
- [Microservices Example](examples/Microservices/)

## Support

For issues, questions, or contributions:
- **GitHub Issues:** [EasyHealth.HealthChecks Issues](https://github.com/easyhealth/healthchecks/issues)
- **Documentation:** [Official Documentation](https://docs.easyhealth.com/healthchecks)
- **Contact:** support@easyhealth.com

## Version History

### v1.0.2 (Current)
- Enterprise-grade code quality with 400+ analyzer rules
- Multi-targeting support (.NET 8 & 9)
- Comprehensive test coverage
- Security hardening and performance optimizations

### v1.0.1
- Initial memory and HTTP health checks
- Basic Kubernetes integration
- Core functionality implementation

---

*Last Updated: December 2024*