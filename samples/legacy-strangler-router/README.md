# Legacy Strangler Router

A .NET 10 cohort router that moves deterministic traffic from a legacy implementation to a modern slice, compares outcomes, and supports immediate rollback.

## What it demonstrates

- modernizing capability by capability using discovery, characterization tests, seams, incremental routing, data migration, observability, and reversible cutovers.
- Modernization changes business capability delivery and operational risk; a framework upgrade or cloud move alone does not remove coupling, unsafe data ownership, or weak delivery practices.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/LegacyStranglerRouter
```

## Check

```powershell
dotnet run --project src/LegacyStranglerRouter -- --self-test
```

## Production boundary

A big-bang rewrite can spend years reproducing undocumented behavior while the legacy system keeps changing and business feedback arrives too late. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
