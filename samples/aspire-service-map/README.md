# Aspire Service Map

A real Aspire 13.4 AppHost coordinates two .NET 10 API resources, injects the catalog reference into the orders resource, waits on health, and exposes the Aspire dashboard for logs, metrics, traces, endpoints, and lifecycle actions.

## Run

Install the stable Aspire CLI, then run:

```powershell
aspire run --project src/AspireServiceMap.AppHost
```

The API can also run independently with `dotnet run --project src/AspireServiceMap.Api`.

## Test

```powershell
dotnet test AspireServiceMap.slnx
```

The AppHost is an application model, not a production hosting platform. Choose managed production resources, identity, networking, scaling, secret delivery, and deployment tooling explicitly for the target environment.
