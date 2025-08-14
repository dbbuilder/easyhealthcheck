# EasyHealth.HealthChecks - Quick Start Guide

Get up and running with EasyHealth.HealthChecks in under 5 minutes!

## 📦 1. Install the Package

Choose your preferred method:

### Package Manager Console
```powershell
Install-Package EasyHealth.HealthChecks -Version 1.0.2
```

### .NET CLI
```bash
dotnet add package EasyHealth.HealthChecks --version 1.0.2
```

### PackageReference
```xml
<PackageReference Include="EasyHealth.HealthChecks" Version="1.0.2" />
```

## 🚀 2. Add to Your Application

### Minimal Setup (Program.cs)
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

That's it! Your application now has health checks enabled.

## 🔍 3. Test Your Health Checks

Start your application and test these endpoints:

```bash
# Overall health status
curl http://localhost:5000/health

# Kubernetes readiness probe
curl http://localhost:5000/health/ready

# Kubernetes liveness probe  
curl http://localhost:5000/health/live
```

### Expected Response
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0156243",
  "entries": {
    "memory_check": {
      "status": "Healthy",
      "description": "Memory usage is within acceptable limits",
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

## ⚙️ 4. Add Configuration (Optional)

Customize your health checks:

```csharp
builder.Services.AddEasyHealthChecks(options =>
{
    // Configure memory monitoring
    options.Memory.ThresholdBytes = 1_073_741_824; // 1GB
    options.Memory.DegradedThresholdBytes = 858_993_459; // 800MB
    
    // Add HTTP endpoint monitoring
    options.Http.Add(new HttpHealthCheckOptions
    {
        Name = "external_api",
        Url = new Uri("https://api.example.com/health"),
        Timeout = TimeSpan.FromSeconds(10),
        SlowResponseThresholdMs = 2000
    });
});
```

## 📊 5. View Health Status

### Simple Health Dashboard
Add this endpoint to view a basic dashboard:

```csharp
app.MapGet("/dashboard", async (IServiceProvider services) =>
{
    var healthCheckService = services.GetRequiredService<HealthCheckService>();
    var result = await healthCheckService.CheckHealthAsync();
    
    return Results.Content($"""
        <html><head><title>Health Dashboard</title></head><body>
        <h1>Application Health: {result.Status}</h1>
        <p>Duration: {result.TotalDuration}</p>
        <ul>
        {string.Join("", result.Entries.Select(e => 
            $"<li><strong>{e.Key}:</strong> {e.Value.Status} - {e.Value.Description}</li>"))}
        </ul>
        </body></html>
        """, "text/html");
});
```

Visit `http://localhost:5000/dashboard` to see your health status.

## 🐳 6. Docker Integration

Add health check to your Dockerfile:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MyApp.dll"]
```

## ☸️ 7. Kubernetes Integration

Add probes to your deployment:

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

## 🎯 What's Next?

### For Production Use
1. **Configure thresholds** for your environment
2. **Add monitoring** for your dependencies
3. **Set up alerting** based on health status
4. **Review security** considerations

### Learn More
- 📖 [Full Integration Guide](INTEGRATION_GUIDE.md)
- 🏗️ [Basic Web API Example](examples/BasicWebApi/)
- ☸️ [Kubernetes Example](examples/KubernetesApp/)
- 🔄 [Microservices Example](examples/Microservices/)

### Available Health Checks
- **Memory Check** - Monitors system memory usage
- **HTTP Check** - Monitors external HTTP endpoints
- **Custom Checks** - Add your own health checks

### Common Configurations

#### High-Memory Applications
```csharp
options.Memory.ThresholdBytes = 4_294_967_296; // 4GB
```

#### Multiple External Dependencies
```csharp
options.Http.AddRange(new[]
{
    new HttpHealthCheckOptions { Name = "database", Url = new Uri("http://db:5432/health") },
    new HttpHealthCheckOptions { Name = "cache", Url = new Uri("http://redis:6379/health") },
    new HttpHealthCheckOptions { Name = "queue", Url = new Uri("http://rabbitmq:15672/health") }
});
```

#### Development vs Production
```csharp
if (builder.Environment.IsProduction())
{
    options.Memory.ThresholdBytes = 2_147_483_648; // 2GB in prod
}
else
{
    options.Memory.ThresholdBytes = 1_073_741_824; // 1GB in dev
}
```

## 🆘 Need Help?

- **Issues:** [GitHub Issues](https://github.com/easyhealth/healthchecks/issues)
- **Documentation:** [Official Docs](https://docs.easyhealth.com/healthchecks)
- **Support:** support@easyhealth.com

---

**Congratulations!** 🎉 You now have enterprise-grade health monitoring in your application. Your ops team will thank you!