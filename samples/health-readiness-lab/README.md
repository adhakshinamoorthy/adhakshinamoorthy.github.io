# Health Readiness Lab

A runnable .NET 10 model of startup/readiness state, bounded required-dependency probes, cancellation, and aggregate health status.

## Run
```powershell
dotnet run --project src/HealthReadinessLab
```
## Test
```powershell
dotnet test HealthReadinessLab.slnx
```

Production ASP.NET Core services should register IHealthCheck implementations, tag liveness/readiness checks, expose separate endpoints, apply strict timeouts, and avoid leaking sensitive dependency details.
