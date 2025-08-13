# EasyHealth.HealthChecks

[![CI/CD Pipeline](https://github.com/dbbuilder/easyhealthcheck/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/dbbuilder/easyhealthcheck/actions/workflows/ci-cd.yml)
[![NuGet Version](https://img.shields.io/nuget/v/EasyHealth.HealthChecks.svg)](https://www.nuget.org/packages/EasyHealth.HealthChecks/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EasyHealth.HealthChecks.svg)](https://www.nuget.org/packages/EasyHealth.HealthChecks/)

A comprehensive health check package for ASP.NET Core applications with minimal configuration and maximum reliability. Always returns results even when the application has issues.

## 🚀 Features

- **🎯 Minimal Configuration**: Add health checks with just one line of code
- **🛡️ Always Reliable**: Never throws exceptions, always returns meaningful results  
- **📊 Built-in Checks**: Memory, disk space, database, HTTP endpoints
- **🔧 Multi-targeting**: Supports .NET 8 and .NET 9
- **📈 Detailed Reporting**: Rich JSON responses with timing and metadata
- **🔧 Extensible**: Easy to add custom health checks

## 📦 Installation

```bash
dotnet add package EasyHealth.HealthChecks
```

## 🏃‍♂️ Quick Start

### Minimal Setup

```csharp
using EasyHealth.HealthChecks.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add health checks with default system monitoring
builder.Services.AddEasyHealthChecks();

var app = builder.Build();

// Add health check endpoint
app.UseEasyHealthChecks();

app.Run();
```

Your application now has health checks at `/health`!

### With Database

```csharp
builder.Services.AddEasyHealthChecks("Server=localhost;Database=MyApp;Integrated Security=true;");
```

### Full Configuration

```csharp
builder.Services.AddEasyHealthChecks(config =>
{
    config.AddDatabase("Server=localhost;Database=MyApp;Integrated Security=true;");
    config.AddMemoryCheck(maxMemoryMB: 2048);
    config.AddDiskSpaceCheck(minFreeSpaceGB: 10);
    config.AddHttpCheck("https://api.external-service.com/health");
});

// Make all health checks resilient (optional)
builder.Services.MakeHealthChecksResilient();

// Add both basic and detailed endpoints
app.UseEasyHealthChecks("/health");
app.UseDetailedEasyHealthChecks("/health/detailed");
```

## 🔍 Built-in Health Checks

### Memory Check
Monitors application memory usage:
- **✅ Healthy**: Memory usage below 80% of limit
- **⚠️ Degraded**: Memory usage between 80% and 100% of limit  
- **❌ Unhealthy**: Memory usage exceeds limit

### Disk Space Check
Monitors available disk space:
- **✅ Healthy**: Free space above 2x minimum requirement
- **⚠️ Degraded**: Free space between 1x and 2x minimum requirement
- **❌ Unhealthy**: Free space below minimum requirement

### Database Check
Verifies database connectivity using Entity Framework health checks.

### HTTP Endpoint Check
Monitors external service availability with configurable timeouts.

## 📋 Health Check Responses

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
Includes additional system information:
- Environment name
- Machine name  
- Process ID
- Health check tags
- Extended metadata

## 🛡️ Resilient Design

EasyHealth.HealthChecks is designed to never fail catastrophically:

- **🚫 No Exceptions**: All health checks are wrapped to prevent exceptions from bubbling up
- ⏱️ **Automatic Timeouts**: Prevent hanging health checks (30s per check, 2m total)
- 📉 **Graceful Degradation**: When checks fail, reports "Degraded" instead of "Unhealthy"
- ✅ **Always Returns**: Even if the entire health check system fails, returns meaningful response

## 🏗️ Architecture

```
EasyHealth.HealthChecks/
├── Core/                          # Core resilience infrastructure
│   ├── ResilientHealthCheck       # Individual check wrapper
│   └── HealthCheckDecorator       # Service-level resilience
├── Checks/                        # Built-in health checks
│   ├── MemoryHealthCheck          # Memory monitoring
│   ├── DiskSpaceHealthCheck       # Disk space monitoring
│   └── HttpHealthCheck            # HTTP endpoint monitoring
└── Extensions/                    # Configuration extensions
    └── ServiceCollectionExtensions # DI setup methods
```

## 🔧 Configuration Options

### Memory Check Options
```csharp
config.AddMemoryCheck(
    maxMemoryMB: 2048,      // Maximum memory before unhealthy
    name: "memory"          // Custom check name
);
```

### Disk Space Check Options  
```csharp
config.AddDiskSpaceCheck(
    minFreeSpaceGB: 10,     // Minimum free space required
    path: "/data",          // Path to monitor (default: current directory)
    name: "disk_space"      // Custom check name
);
```

### HTTP Check Options
```csharp  
config.AddHttpCheck(
    url: "https://api.example.com/health",
    name: "external_api"    // Custom check name
);
```

### Database Check Options
```csharp
config.AddDatabase(
    connectionString: "Server=localhost;Database=MyApp;Integrated Security=true;",
    name: "database"        // Custom check name
);
```

## 📚 Best Practices

1. **✅ Use in Production**: The resilient design makes it safe for production monitoring
2. **📊 Monitor Trends**: Use the detailed endpoint for monitoring systems that track trends
3. **⚙️ Set Appropriate Limits**: Configure memory and disk space limits based on your application's needs
4. **🌐 External Dependencies**: Use HTTP checks for critical external services
5. **🗄️ Database Monitoring**: Always include database health checks for data-driven applications
6. **🔄 Regular Testing**: Test your health checks in different failure scenarios

## 🚀 Deployment

The package automatically creates a NuGet package on build. For manual publishing:

```bash
dotnet pack --configuration Release
dotnet nuget push bin/Release/EasyHealth.HealthChecks.*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- 📖 [Documentation](https://github.com/dbbuilder/easyhealthcheck/wiki)
- 🐛 [Issues](https://github.com/dbbuilder/easyhealthcheck/issues)
- 💬 [Discussions](https://github.com/dbbuilder/easyhealthcheck/discussions)

## ⭐ Show Your Support

If you find this package useful, please give it a star! ⭐