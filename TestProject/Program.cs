using EasyHealth.HealthChecks.Extensions;
using EasyHealth.HealthChecks.Core;

var builder = WebApplication.CreateBuilder(args);

// Test the EasyHealth.HealthChecks package
builder.Services.AddEasyHealthChecks(config =>
{
    config.AddMemoryCheck(maxMemoryMB: 1024);
    config.AddDiskSpaceCheck(minFreeSpaceGB: 1);
    config.AddHttpCheck("https://httpbin.org/status/200");
});

// Make health checks resilient
builder.Services.MakeHealthChecksResilient();

var app = builder.Build();

// Add health check endpoints
app.UseEasyHealthChecks("/health");
app.UseDetailedEasyHealthChecks("/health/detailed");

app.MapGet("/", () => "EasyHealth.HealthChecks test application running! Check /health");

app.Run();