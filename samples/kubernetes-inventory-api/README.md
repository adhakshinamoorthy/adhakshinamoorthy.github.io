# Kubernetes Inventory API

A runnable .NET 10 service and production-oriented Kubernetes manifests demonstrating startup, readiness and liveness semantics, graceful shutdown, safe rolling updates, resource controls, pod security, disruption protection, and autoscaling.

## Run

```powershell
dotnet run --project src/KubernetesInventoryApi
```

Probe `http://localhost:5000/livez` and `http://localhost:5000/readyz` using the port printed by ASP.NET Core.

## Test

```powershell
dotnet test KubernetesInventoryApi.slnx
```

## Deploy to a development cluster

Build and push the image named in `k8s/deployment.yaml`, then run:

```powershell
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/autoscaling.yaml
kubectl rollout status deployment/inventory-api
```

Production deployments should use an immutable image digest, an external secret provider, namespace-level policy, telemetry, verified resource values, and a tested rollback procedure.
