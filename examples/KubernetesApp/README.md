# Kubernetes Integration Example

This example demonstrates how to integrate EasyHealth.HealthChecks with Kubernetes, including proper probe configuration and container optimization.

## Features Demonstrated

- Kubernetes liveness, readiness, and startup probes
- Container-optimized health check configuration
- Multi-service dependency monitoring
- Docker health checks
- Prometheus metrics integration
- Graceful shutdown handling

## Quick Start

### 1. Build and Deploy Locally

```bash
# Build the Docker image
docker build -t easyhealth/demo:latest .

# Run locally
docker run -p 8080:8080 easyhealth/demo:latest

# Test health endpoints
curl http://localhost:8080/health
curl http://localhost:8080/health/ready
curl http://localhost:8080/health/live
```

### 2. Deploy to Kubernetes

```bash
# Apply the deployment
kubectl apply -f deployment.yaml

# Check deployment status
kubectl get pods -l app=easyhealth-demo

# Check health check status
kubectl describe pod <pod-name>

# Port forward to test locally
kubectl port-forward svc/easyhealth-demo-service 8080:80

# Test the endpoints
curl http://localhost:8080/health
```

## Kubernetes Configuration

### Probe Configuration

The deployment includes three types of probes:

#### Liveness Probe
- **Purpose:** Determines if the container should be restarted
- **Endpoint:** `/health/live`
- **Initial Delay:** 30 seconds
- **Period:** 10 seconds
- **Failure Threshold:** 3 consecutive failures

#### Readiness Probe
- **Purpose:** Determines if the pod should receive traffic
- **Endpoint:** `/health/ready`
- **Initial Delay:** 10 seconds
- **Period:** 5 seconds
- **Failure Threshold:** 3 consecutive failures

#### Startup Probe
- **Purpose:** Gives extra time during application startup
- **Endpoint:** `/health/live`
- **Failure Threshold:** 30 (allows up to 2.5 minutes for startup)

### Health Check Configuration for Containers

```csharp
options.Memory.ThresholdBytes = 400_000_000; // 400MB (below container limit)
options.Memory.DegradedThresholdBytes = 300_000_000; // 300MB

// Microservice dependency checks
options.Http.Add(new HttpHealthCheckOptions
{
    Name = "user_service",
    Url = new Uri("http://user-service:80/health"),
    Timeout = TimeSpan.FromSeconds(5)
});
```

## Monitoring Integration

### Prometheus Metrics

The deployment includes a ServiceMonitor for Prometheus:

```yaml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: easyhealth-demo-metrics
spec:
  endpoints:
  - port: http
    path: /health
    interval: 30s
```

### Viewing Metrics

```bash
# If Prometheus is deployed, you can query health check metrics
curl http://prometheus:9090/api/v1/query?query=up{job="easyhealth-demo-metrics"}
```

## Testing Different Scenarios

### 1. Test Pod Restart (Liveness Failure)

Simulate an unhealthy application:

```bash
# Create a temporary pod that will fail health checks
kubectl run unhealthy-pod --image=easyhealth/demo:latest --env="FORCE_UNHEALTHY=true"

# Watch the pod get restarted
kubectl get pods -w
```

### 2. Test Traffic Routing (Readiness Failure)

```bash
# Scale deployment to multiple replicas
kubectl scale deployment easyhealth-demo --replicas=3

# Simulate readiness failure on one pod
kubectl exec <pod-name> -- curl -X POST http://localhost:8080/admin/ready/false

# Verify traffic only goes to ready pods
kubectl get endpoints easyhealth-demo-service
```

### 3. Monitor Health Status

```bash
# Check health status of all pods
for pod in $(kubectl get pods -l app=easyhealth-demo -o name); do
  echo "=== $pod ==="
  kubectl exec $pod -- curl -s http://localhost:8080/health | jq .status
done
```

## Troubleshooting

### Common Issues

#### Pod Keeps Restarting
```bash
# Check events and logs
kubectl describe pod <pod-name>
kubectl logs <pod-name> --previous

# Common causes:
# - Memory threshold too low for container
# - External dependencies not available
# - Network connectivity issues
```

#### Service Not Ready
```bash
# Check readiness probe logs
kubectl logs <pod-name> | grep "health/ready"

# Check service endpoints
kubectl describe endpoints easyhealth-demo-service

# Verify pod is marked as ready
kubectl get pods -o wide
```

#### Health Checks Timing Out
```bash
# Increase probe timeout in deployment.yaml
spec:
  containers:
  - livenessProbe:
      timeoutSeconds: 10  # Increase from 5
```

### Performance Tuning

#### Memory-Constrained Environments
```csharp
// Adjust thresholds for smaller containers
options.Memory.ThresholdBytes = 200_000_000; // 200MB
options.Memory.DegradedThresholdBytes = 150_000_000; // 150MB
```

#### High-Traffic Scenarios
```csharp
// Reduce probe frequency
// In deployment.yaml:
readinessProbe:
  periodSeconds: 10  # Increase from 5
  timeoutSeconds: 5
```

## Security Considerations

### Network Policies

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: easyhealth-demo-netpol
spec:
  podSelector:
    matchLabels:
      app: easyhealth-demo
  policyTypes:
  - Ingress
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: ingress-controller
    ports:
    - protocol: TCP
      port: 8080
```

### RBAC (if needed)

```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: easyhealth-demo
---
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: easyhealth-demo
rules:
- apiGroups: [""]
  resources: ["pods"]
  verbs: ["get", "list"]
```

## Advanced Configuration

### Custom Health Check Policies

```csharp
// Different policies for different environments
if (builder.Environment.IsProduction())
{
    options.Memory.ThresholdBytes = 800_000_000; // Higher threshold in prod
    // More conservative timeouts
    options.Http.ForEach(h => h.Timeout = TimeSpan.FromSeconds(10));
}
```

### Integration with Service Mesh

For Istio/Linkerd integration:

```yaml
metadata:
  annotations:
    prometheus.io/scrape: "true"
    prometheus.io/path: "/health"
    prometheus.io/port: "8080"
```

This example provides a complete foundation for deploying EasyHealth.HealthChecks in production Kubernetes environments.