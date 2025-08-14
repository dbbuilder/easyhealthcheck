# Microservices Example

This example demonstrates how to use EasyHealth.HealthChecks in a microservices architecture with multiple services monitoring each other's health.

## Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   API Gateway   │    │  User Service   │    │Payment Service  │
│                 │    │                 │    │                 │
│ Port: 5000      │    │ Port: 5001      │    │ Port: 5002      │
│ Health: /health │◄──►│ Health: /health │◄──►│ Health: /health │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 ▼
                    ┌─────────────────┐
                    │ Notification    │
                    │ Service         │
                    │ Port: 5003      │
                    │ Health: /health │
                    └─────────────────┘
```

## Services Overview

### API Gateway (`ApiGateway/`)
- **Port:** 5000
- **Purpose:** Entry point for all client requests
- **Health Checks:** Monitors all downstream services
- **Dependencies:** User Service, Payment Service, Notification Service

### User Service (`UserService/`)
- **Port:** 5001
- **Purpose:** User management and authentication
- **Health Checks:** Memory, database connection (simulated)
- **Dependencies:** Notification Service

### Payment Service (`PaymentService/`)
- **Port:** 5002
- **Purpose:** Payment processing
- **Health Checks:** Memory, external payment gateway (simulated)
- **Dependencies:** User Service, Notification Service

### Notification Service (`NotificationService/`)
- **Port:** 5003
- **Purpose:** Email and SMS notifications
- **Health Checks:** Memory, external SMTP service (simulated)
- **Dependencies:** None (leaf service)

## Running the Example

### Prerequisites
- .NET 8.0 SDK
- Docker (optional)
- PowerShell or Bash

### Option 1: Run with Scripts

#### Windows (PowerShell)
```powershell
# Start all services
.\scripts\start-services.ps1

# Check health status of all services
.\scripts\check-health.ps1

# Stop all services
.\scripts\stop-services.ps1
```

#### Linux/macOS (Bash)
```bash
# Make scripts executable
chmod +x scripts/*.sh

# Start all services
./scripts/start-services.sh

# Check health status
./scripts/check-health.sh

# Stop all services
./scripts/stop-services.sh
```

### Option 2: Manual Startup

```bash
# Terminal 1 - Notification Service (start first, no dependencies)
cd NotificationService
dotnet run

# Terminal 2 - User Service
cd UserService
dotnet run

# Terminal 3 - Payment Service
cd PaymentService
dotnet run

# Terminal 4 - API Gateway
cd ApiGateway
dotnet run
```

### Option 3: Docker Compose

```bash
# Build and start all services
docker-compose up --build

# Check logs
docker-compose logs -f

# Stop services
docker-compose down
```

## Testing Health Checks

### Check Individual Services

```bash
# Notification Service (no dependencies)
curl http://localhost:5003/health | jq

# User Service (depends on Notification)
curl http://localhost:5001/health | jq

# Payment Service (depends on User and Notification)
curl http://localhost:5002/health | jq

# API Gateway (depends on all services)
curl http://localhost:5000/health | jq
```

### Expected Healthy Response

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "memory_check": {
      "status": "Healthy",
      "description": "Memory usage is within acceptable limits"
    },
    "user_service": {
      "status": "Healthy", 
      "description": "http://localhost:5001/health responded successfully in 45ms"
    },
    "payment_service": {
      "status": "Healthy",
      "description": "http://localhost:5002/health responded successfully in 32ms"
    },
    "notification_service": {
      "status": "Healthy",
      "description": "http://localhost:5003/health responded successfully in 28ms"
    }
  }
}
```

## Health Check Configuration

### API Gateway Configuration

```csharp
builder.Services.AddEasyHealthChecks(options =>
{
    options.Memory.ThresholdBytes = 512_000_000; // 512MB
    
    // Monitor all downstream services
    options.Http.AddRange(new[]
    {
        new HttpHealthCheckOptions
        {
            Name = "user_service",
            Url = new Uri("http://localhost:5001/health"),
            Tags = new[] { "microservice", "critical" }
        },
        new HttpHealthCheckOptions
        {
            Name = "payment_service", 
            Url = new Uri("http://localhost:5002/health"),
            Tags = new[] { "microservice", "critical" }
        },
        new HttpHealthCheckOptions
        {
            Name = "notification_service",
            Url = new Uri("http://localhost:5003/health"),
            Tags = new[] { "microservice", "non-critical" }
        }
    });
});
```

### Service-Specific Configuration

Each service has tailored health check configuration:

```csharp
// User Service - checks database simulation
options.Http.Add(new HttpHealthCheckOptions
{
    Name = "user_database",
    Url = new Uri("http://localhost:5432/health"), // Simulated DB
    Tags = new[] { "database", "critical" }
});

// Payment Service - checks payment gateway simulation  
options.Http.Add(new HttpHealthCheckOptions
{
    Name = "payment_gateway",
    Url = new Uri("https://api.stripe.com/v1/charges"), // Example
    ExpectedStatusCode = HttpStatusCode.Unauthorized, // Expected without API key
    Tags = new[] { "external", "critical" }
});
```

## Failure Scenarios

### Simulate Service Failures

#### 1. Stop Notification Service
```bash
# Stop notification service
curl -X POST http://localhost:5003/admin/shutdown

# Check cascade effect
curl http://localhost:5001/health | jq .status  # May show Degraded
curl http://localhost:5002/health | jq .status  # May show Degraded  
curl http://localhost:5000/health | jq .status  # May show Unhealthy
```

#### 2. Memory Pressure Simulation
```bash
# Trigger high memory usage
curl -X POST http://localhost:5001/admin/memory/high

# Check health status
curl http://localhost:5001/health | jq
```

#### 3. Network Latency Simulation
```bash
# Add artificial delay
curl -X POST http://localhost:5002/admin/delay/5000

# Check if marked as degraded
curl http://localhost:5000/health | jq
```

## Monitoring and Alerting

### Health Dashboard

Access the centralized health dashboard:
```
http://localhost:5000/health/dashboard
```

This shows:
- Overall system health
- Individual service status
- Response times
- Dependency graph

### Prometheus Metrics

If running with Docker Compose, Prometheus is available at:
```
http://localhost:9090
```

Sample queries:
```promql
# Service availability
up{job="microservices"}

# Health check duration
health_check_duration_seconds

# Service dependency health
health_check_status{service="user_service"}
```

### Grafana Dashboard

Access Grafana at:
```
http://localhost:3000
```

Default credentials: `admin/admin`

Pre-configured dashboards include:
- Microservices Health Overview
- Service Dependency Map
- Response Time Trends
- Error Rate Analysis

## Circuit Breaker Pattern

The example includes circuit breaker integration:

```csharp
// In PaymentService
builder.Services.AddEasyHealthChecks(options =>
{
    options.Http.Add(new HttpHealthCheckOptions
    {
        Name = "user_service",
        Url = new Uri("http://localhost:5001/health"),
        FailureThreshold = 3, // Open circuit after 3 failures
        TimeoutSeconds = 5,
        Tags = new[] { "circuit-breaker" }
    });
});
```

## Load Testing

### Artillery Load Test

```bash
# Install artillery
npm install -g artillery

# Run load test
artillery run scripts/load-test.yml

# Check health during load
watch -n 1 'curl -s http://localhost:5000/health | jq .status'
```

### Expected Behavior Under Load

- Services should maintain "Healthy" status under normal load
- May show "Degraded" when response times increase
- Should never show "Unhealthy" unless actual failures occur

## Deployment Strategies

### Rolling Deployment Simulation

```bash
# Script to simulate rolling deployment
for service in notification user payment gateway; do
    echo "Deploying $service..."
    # Stop service
    curl -X POST http://localhost:500${service_port}/admin/shutdown
    sleep 5
    # Start new version (simulated by restart)
    # Health checks should detect and route around downtime
    sleep 10
done
```

### Blue-Green Deployment

The example includes configuration for blue-green deployments:

```yaml
# docker-compose.blue-green.yml
version: '3.8'
services:
  api-gateway-blue:
    # Blue environment
  api-gateway-green:
    # Green environment
  load-balancer:
    # Routes based on health checks
```

## Best Practices Demonstrated

### 1. Graceful Degradation
- Non-critical services (notifications) don't fail the entire system
- Services can operate with reduced functionality

### 2. Health Check Hierarchies
- Leaf services (notification) have minimal health checks
- Gateway services have comprehensive dependency monitoring

### 3. Timeout Configuration
- Different timeouts for different service criticality
- Shorter timeouts for non-critical dependencies

### 4. Tag-Based Monitoring
- Services tagged by criticality and type
- Enables filtering and targeted alerting

### 5. Circuit Breaking
- Prevents cascade failures
- Automatic recovery when services come back online

## Troubleshooting

### Common Issues

#### All Services Show Unhealthy
- Check if services are running on correct ports
- Verify firewall/network connectivity
- Check service startup order

#### Intermittent Health Check Failures
- Review timeout configurations
- Check for memory pressure
- Monitor network latency

#### Dashboard Not Loading
- Ensure API Gateway is running
- Check browser console for errors
- Verify health check endpoints are responding

This example provides a comprehensive demonstration of EasyHealth.HealthChecks in a realistic microservices environment, showcasing enterprise patterns and best practices.