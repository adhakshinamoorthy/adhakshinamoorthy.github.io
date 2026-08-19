# CAF Landing Zone Readiness

A .NET 10 landing-zone readiness assessment that scores identity, organization, network, policy, operations, security, cost, and recovery gates.

## What it demonstrates

- aligning cloud strategy, planning, landing zones, governance, security, operations, workload migration, and measurable business outcomes.
- The Cloud Adoption Framework supplies guidance and decision structure; each organization must tailor it to regulations, operating model, skills, portfolio, risk appetite, and workload needs.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/CafLandingZoneReadiness
```

## Check

```powershell
dotnet run --project src/CafLandingZoneReadiness -- --self-test
```

## Production boundary

Migrating workloads before identity, connectivity, policy, ownership, budgets, logging, recovery, and support are ready reproduces legacy risk at cloud speed. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
