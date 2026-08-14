# Container Apps revision API

A .NET 10 API prepared for Azure Container Apps with immutable revision metadata, separate liveness and readiness endpoints, graceful drain behavior, JSON logs, a non-root chiseled image, managed identity, HTTPS ingress, explicit probes, and measured HTTP scaling settings.

## Run and test

```powershell
dotnet run --project src/ContainerAppsRevisionApi
dotnet test ContainerAppsRevisionApi.slnx
```

## Container smoke test

```powershell
docker build -t container-apps-revision-api .
docker run --rm -p 8080:8080 container-apps-revision-api
```

Call `/`, `/health/live`, and `/health/ready`. Deploy `infra/main.bicep` only after supplying an approved image from a private registry and a governed Log Analytics workspace. Configure registry access with managed identity, add network controls, alerts, budgets, diagnostic settings, and revision traffic gates in the delivery environment.
