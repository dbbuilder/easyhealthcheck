# EasyHealth.HealthChecks

[![NuGet Version](https://img.shields.io/nuget/v/EasyHealth.HealthChecks.svg)](https://www.nuget.org/packages/EasyHealth.HealthChecks/)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/easyhealth/healthchecks)

Enterprise-grade health monitoring for .NET applications with zero configuration required. Built with 400+ code quality analyzers, comprehensive testing, and production-ready defaults.

## ✨ Features

- **🚀 Zero Configuration** - Works out of the box with sensible defaults
- **💾 Memory Monitoring** - Automatic memory usage tracking with configurable thresholds
- **🌐 HTTP Health Checks** - Monitor external dependencies and APIs
- **☸️ Kubernetes Ready** - Built-in liveness, readiness, and startup probe endpoints
- **🐳 Docker Compatible** - Integrated health check support
- **📊 Rich Diagnostics** - Detailed health status with performance metrics
- **🔒 Enterprise Security** - Security-hardened with comprehensive code analysis
- **⚡ High Performance** - Optimized with LoggerMessage delegates and async patterns
- **🎯 Multi-Targeting** - Supports .NET 8.0 and .NET 9.0

## 🚀 Quick Start

### Installation

```bash
dotnet add package EasyHealth.HealthChecks
```

### Basic Setup

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

### Test Your Health Checks

```bash
curl http://localhost:5000/health
curl http://localhost:5000/health/ready
curl http://localhost:5000/health/live
```

**That's it!** Your application now has enterprise-grade health monitoring.

## 📊 Sample Response

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

## ⚙️ Configuration

### Memory Monitoring

```csharp
builder.Services.AddEasyHealthChecks(options =>
{
    options.Memory.ThresholdBytes = 1_073_741_824; // 1GB
    options.Memory.DegradedThresholdBytes = 858_993_459; // 800MB
    options.Memory.Tags = new[] { "memory", "system" };
});
```

### HTTP Endpoint Monitoring

```csharp
builder.Services.AddEasyHealthChecks(options =>
{
    options.Http.Add(new HttpHealthCheckOptions
    {
        Name = "external_api",
        Url = new Uri("https://api.example.com/health"),
        Timeout = TimeSpan.FromSeconds(30),
        ExpectedStatusCode = HttpStatusCode.OK,
        SlowResponseThresholdMs = 2000,
        Tags = new[] { "external", "critical" }
    });
});
```

### Environment-Specific Configuration

```csharp
builder.Services.AddEasyHealthChecks(options =>
{
    if (builder.Environment.IsProduction())
    {
        options.Memory.ThresholdBytes = 4_294_967_296; // 4GB
        options.Http.Add(new HttpHealthCheckOptions
        {
            Name = "prod_database",
            Url = new Uri("https://prod-db.company.com/health")
        });
    }
    else
    {
        options.Memory.ThresholdBytes = 1_073_741_824; // 1GB
        options.Http.Add(new HttpHealthCheckOptions
        {
            Name = "dev_database", 
            Url = new Uri("https://dev-db.company.com/health")
        });
    }
});
```

## 🐳 Docker Integration

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MyApp.dll"]
```

## ☸️ Kubernetes Integration

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
        startupProbe:
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
          failureThreshold: 30
```

## 📈 Available Health Checks

### Memory Health Check
- **Purpose:** Monitor system memory usage
- **Thresholds:** Configurable healthy/degraded/unhealthy limits
- **Metrics:** Allocated bytes, working set, usage percentage
- **Default Threshold:** 1GB

### HTTP Health Check
- **Purpose:** Monitor external HTTP endpoints
- **Features:** Configurable timeouts, expected status codes, response time monitoring
- **Metrics:** Response time, status code, success rate
- **Default Timeout:** 30 seconds

### Custom Health Checks
Easily add your own health checks by implementing `IHealthCheck`:

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        // Your custom health check logic
        var isHealthy = await CheckDatabaseConnection();
        
        return isHealthy 
            ? HealthCheckResult.Healthy("Database connection successful")
            : HealthCheckResult.Unhealthy("Database connection failed");
    }
}

// Register your custom health check
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");
```

## 📚 Documentation

- **[🚀 Quick Start Guide](QUICK_START.md)** - Get up and running in 5 minutes
- **[📖 Integration Guide](INTEGRATION_GUIDE.md)** - Comprehensive implementation guide
- **[🏗️ Basic Web API Example](examples/BasicWebApi/)** - Simple integration example
- **[☸️ Kubernetes Example](examples/KubernetesApp/)** - Production Kubernetes deployment
- **[🔄 Microservices Example](examples/Microservices/)** - Multi-service architecture

## 🔒 Security & Quality

EasyHealth.HealthChecks is built with enterprise-grade quality standards:

- **400+ Code Analyzers** - Comprehensive static analysis
- **Security Hardened** - No banned APIs, secure coding practices
- **Performance Optimized** - LoggerMessage delegates, async patterns
- **Comprehensive Testing** - 16+ unit tests, 100% coverage on critical paths
- **Multi-Target Support** - .NET 8.0 and .NET 9.0

## 🏗️ Enterprise Features

### Security Considerations

```csharp
// Restrict access in production
app.MapEasyHealthChecks()
   .RequireAuthorization("HealthCheckPolicy");

// Network-level protection
app.MapEasyHealthChecks()
   .RequireHost("internal.company.com");
```

### Monitoring Integration

```csharp
// Application Insights integration
builder.Services.AddEasyHealthChecks()
    .AddApplicationInsightsPublisher();

// Custom alerting
builder.Services.AddSingleton<IHealthCheckPublisher, CustomAlertPublisher>();
```

### Performance Tuning

```csharp
// Configure caching for high-traffic scenarios
builder.Services.Configure<HealthCheckOptions>(options =>
{
    options.ResultStatusCodes[HealthStatus.Healthy] = StatusCodes.Status200OK;
    options.ResultStatusCodes[HealthStatus.Degraded] = StatusCodes.Status200OK;
    options.ResultStatusCodes[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable;
});
```

## 🎯 Use Cases

### Microservices Architecture
Monitor service dependencies and cascade health status appropriately.

### Container Orchestration
Integrate with Kubernetes, Docker Swarm, or other orchestrators for automated health monitoring.

### Load Balancer Integration
Provide health endpoints for load balancers to route traffic only to healthy instances.

### Monitoring & Alerting
Feed health data into monitoring systems like Prometheus, Grafana, or Application Insights.

### CI/CD Pipelines
Use health checks for deployment validation and automated rollback triggers.

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

```bash
git clone https://github.com/easyhealth/healthchecks.git
cd healthchecks
dotnet restore
dotnet build
dotnet test
```

## 📋 Requirements

- **.NET 8.0** or **.NET 9.0**
- **ASP.NET Core** application
- **Microsoft.Extensions.Diagnostics.HealthChecks** (automatically included)

## 🔄 Version History

### v1.0.2 (Current)
- ✨ Enterprise-grade code quality with 400+ analyzer rules
- 🎯 Multi-targeting support (.NET 8 & 9)
- 🔒 Security hardening and performance optimizations
- 📊 Comprehensive test coverage
- 🚀 Production-ready defaults

### v1.0.1
- 💾 Memory and HTTP health checks
- ☸️ Basic Kubernetes integration
- 🏗️ Core functionality

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **🐛 Issues:** [GitHub Issues](https://github.com/easyhealth/healthchecks/issues)
- **📖 Documentation:** [Official Documentation](https://docs.easyhealth.com/healthchecks)
- **💬 Discussions:** [GitHub Discussions](https://github.com/easyhealth/healthchecks/discussions)
- **📧 Email:** support@easyhealth.com

## ⭐ Show Your Support

If EasyHealth.HealthChecks helps your project, please consider giving it a star on GitHub! ⭐

---

**Built with ❤️ by the EasyHealth team for the .NET community**