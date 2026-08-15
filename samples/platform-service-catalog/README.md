# Platform Service Catalog

A .NET 10 service-catalog validator that checks ownership, tier, repository, runtime, health, telemetry, and runbook metadata before onboarding.

## What it demonstrates

- a product-managed platform that gives teams self-service golden paths for creation, delivery, security, observability, ownership, and lifecycle management.
- An internal developer platform reduces repeated cognitive load through paved roads and APIs; it is not merely a portal, a Kubernetes cluster, or a centralized ticket queue.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/PlatformServiceCatalog
```

## Check

```powershell
dotnet run --project src/PlatformServiceCatalog -- --self-test
```

## Production boundary

A platform built without user research becomes mandatory infrastructure that shifts toil, hides unsafe defaults, and cannot demonstrate improved delivery outcomes. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
