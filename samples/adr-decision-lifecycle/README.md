# ADR Decision Lifecycle

A .NET 10 ADR catalog validator that detects missing owners, invalid status transitions, and accepted decisions whose superseding record is absent.

## What it demonstrates

- capturing significant architectural decisions with context, options, consequences, status, evidence, and supersession history close to the code.
- An ADR records why a consequential choice was made; it complements diagrams, standards, code, and operational evidence rather than duplicating them.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/AdrDecisionLifecycle
```

## Check

```powershell
dotnet run --project src/AdrDecisionLifecycle -- --self-test
```

## Production boundary

Decision records become ceremony when they are vague, retrospective, disconnected from changes, or silently edited after teams depend on them. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
