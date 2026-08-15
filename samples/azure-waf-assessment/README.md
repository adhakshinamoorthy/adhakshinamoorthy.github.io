# Azure Well-Architected Assessment

A .NET 10 five-pillar workload assessment that weights findings by critical flow, impact, likelihood, evidence, owner, and remediation status.

## What it demonstrates

- evaluating Azure workload decisions across reliability, security, cost optimization, operational excellence, and performance efficiency as connected trade-offs.
- The Well-Architected Framework guides workload decisions and reviews; Azure service compliance or a one-time assessment does not prove that a workload is well architected.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/AzureWafAssessment
```

## Check

```powershell
dotnet run --project src/AzureWafAssessment -- --self-test
```

## Production boundary

Optimizing one pillar in isolation can shift unacceptable risk to another—for example, reducing redundancy may save cost while violating recovery objectives. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
