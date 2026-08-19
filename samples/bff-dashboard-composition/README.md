# BFF Dashboard Composition

A .NET 10 dashboard composer with parallel bounded calls, user-scoped caching, partial-failure reporting, and a presentation-specific response.

## What it demonstrates

- a client-specific backend that composes downstream capabilities, protects browser credentials, and shapes contracts around one user experience.
- A BFF owns client orchestration and presentation-shaped contracts; domain rules and source-of-truth data remain in downstream services.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/BffDashboardComposition
```

## Check

```powershell
dotnet run --project src/BffDashboardComposition -- --self-test
```

## Production boundary

An oversized BFF becomes a second monolith, duplicates business logic, fans out without deadlines, or exposes cookies and tokens through weak browser controls. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
