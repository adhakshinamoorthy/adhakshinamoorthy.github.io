# Architecture Quality Attribute Workshop

A .NET 10 workshop model that validates quality-attribute scenarios, ranks architectural risk, and maps measurable responses to candidate tactics.

## What it demonstrates

- turning business goals and constraints into measurable quality-attribute scenarios, explicit trade-offs, bounded system responsibilities, and evolutionary decisions.
- Solution architecture connects product, software, data, integration, security, infrastructure, and operations; it does not replace detailed design or accountable delivery teams.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/ArchitectureQualityAttributes
```

## Check

```powershell
dotnet run --project src/ArchitectureQualityAttributes -- --self-test
```

## Production boundary

A diagram-led design can look complete while omitting measurable availability, latency, security, recovery, cost, ownership, and change scenarios. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
