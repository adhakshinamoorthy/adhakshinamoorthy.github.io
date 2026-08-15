# WAF Operations Release Readiness

A .NET 10 release-readiness gate that verifies ownership, SLOs, dashboards, alerts, runbooks, rollback, capacity, security, and recovery evidence.

## What it demonstrates

- operating Azure workloads through observable standards, safe automation, deployment discipline, actionable alerts, runbooks, learning reviews, and continuous improvement.
- Operational excellence is designed into the workload and delivery system; an operations team cannot add it after development through dashboards alone.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/WafOperationsReleaseReadiness
```

## Check

```powershell
dotnet run --project src/WafOperationsReleaseReadiness -- --self-test
```

## Production boundary

Manual, undocumented, high-privilege changes create configuration drift, slow recovery, inconsistent environments, and incidents whose cause cannot be reconstructed. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
