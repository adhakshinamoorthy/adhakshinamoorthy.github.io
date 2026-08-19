# Docker Orders API

A runnable .NET 10 API showing a cache-friendly multi-stage image, a non-root runtime, health checks, runtime configuration, a read-only filesystem, and a small Compose topology.

## Run locally

```powershell
dotnet run --project src/DockerOrdersApi
```

## Run as a container

```powershell
docker compose up --build
```

Open `http://localhost:8080/health/ready` or create an order with `POST /orders` and JSON `{ "customer": "Ada", "total": 42.50 }`.

## Test

```powershell
dotnet test DockerOrdersApi.slnx
```

The in-memory store is intentionally disposable. Production containers should keep durable state in managed services or explicit volumes, inject secrets at runtime, pin trusted base images by digest where required, and scan the final image in CI.
