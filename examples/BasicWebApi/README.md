# Basic Web API Example

This example demonstrates the simplest integration of EasyHealth.HealthChecks into an ASP.NET Core Web API.

## Features Demonstrated

- Basic health check setup
- Memory monitoring
- External HTTP endpoint monitoring
- Swagger integration
- Multiple health check endpoints

## Running the Example

1. **Navigate to the example directory:**
   ```bash
   cd examples/BasicWebApi
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

4. **Access the endpoints:**
   - API: https://localhost:7243/api/sample
   - Swagger: https://localhost:7243/swagger
   - Health: https://localhost:7243/health
   - Ready: https://localhost:7243/health/ready
   - Live: https://localhost:7243/health/live

## Health Check Endpoints

### `/health` - Overall Health Status
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
        "WorkingSetBytes": 123456789
      }
    },
    "jsonplaceholder_api": {
      "status": "Healthy", 
      "description": "https://jsonplaceholder.typicode.com/posts/1 responded successfully in 234ms",
      "data": {
        "Url": "https://jsonplaceholder.typicode.com/posts/1",
        "StatusCode": 200,
        "ResponseTimeMs": 234
      }
    }
  }
}
```

### `/health/ready` - Readiness Probe
Returns 200 OK when application is ready to receive traffic.

### `/health/live` - Liveness Probe  
Returns 200 OK when application is alive and running.

## Configuration

The example configures:

- **Memory threshold:** 1GB (unhealthy above this)
- **Degraded threshold:** 800MB (degraded above this)
- **External API check:** JSONPlaceholder API
- **Slow response threshold:** 2 seconds

## Testing Different Health States

### Test Degraded Memory State
Reduce the memory threshold to trigger degraded state:
```csharp
options.Memory.DegradedThresholdBytes = 100_000; // Very low threshold
```

### Test Unhealthy HTTP State
Change the URL to a non-existent endpoint:
```csharp
options.Http.Add(new HttpHealthCheckOptions
{
    Name = "broken_api",
    Url = new Uri("https://non-existent-api.example.com/health")
});
```

## Integration with Monitoring

Add logging to see health check results:
```csharp
builder.Logging.AddFilter("EasyHealth.HealthChecks", LogLevel.Information);
```

View detailed health check logs in the console output.