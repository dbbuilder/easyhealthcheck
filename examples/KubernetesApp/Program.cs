using EasyHealth.HealthChecks.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Configure EasyHealth for Kubernetes environment
builder.Services.AddEasyHealthChecks(options =>
{
    // Memory configuration for containerized environment
    options.Memory.ThresholdBytes = 400_000_000; // 400MB (below container limit)
    options.Memory.DegradedThresholdBytes = 300_000_000; // 300MB
    options.Memory.Tags = new[] { "memory", "container", "critical" };
    
    // Health checks for dependent services
    options.Http.AddRange(new[]
    {
        new HttpHealthCheckOptions
        {
            Name = "user_service",
            Url = new Uri("http://user-service:80/health"),
            Timeout = TimeSpan.FromSeconds(5),
            SlowResponseThresholdMs = 1000,
            Tags = new[] { "microservice", "critical" }
        },
        new HttpHealthCheckOptions
        {
            Name = "notification_service",
            Url = new Uri("http://notification-service:80/health"),
            Timeout = TimeSpan.FromSeconds(5),
            SlowResponseThresholdMs = 2000,
            Tags = new[] { "microservice", "non-critical" }
        }
    });
});

// Add logging
builder.Logging.AddConsole();
builder.Logging.AddFilter("EasyHealth.HealthChecks", LogLevel.Information);

var app = builder.Build();

// Configure pipeline
app.UseRouting();

// Map health checks with Kubernetes-optimized configuration
app.MapEasyHealthChecks();

// Add a simple endpoint for testing
app.MapGet("/", () => new { 
    Service = "EasyHealth Kubernetes Demo",
    Version = "1.0.2",
    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    Timestamp = DateTime.UtcNow
});

// Map controllers
app.MapControllers();

// Graceful shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    app.Logger.LogInformation("Application is shutting down gracefully...");
});

app.Run();