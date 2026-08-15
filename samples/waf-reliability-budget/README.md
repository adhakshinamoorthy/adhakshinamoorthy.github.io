# WAF Reliability Budget

A .NET 10 SLO calculator that derives monthly error budget, measures consumption, and raises fast- and slow-burn alerts.

## What it demonstrates

- defining reliability requirements, failure modes, redundancy, recovery, graceful degradation, and operational learning for critical Azure workload flows.
- Reliability is an end-to-end workload property; choosing an availability-zone-capable service does not create a reliable user journey by itself.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/WafReliabilityBudget
```

## Check

```powershell
dotnet run --project src/WafReliabilityBudget -- --self-test
```

## Production boundary

Undefined SLOs and recovery objectives lead teams to buy redundancy without knowing whether failover, data recovery, dependencies, and operations meet the business need. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
