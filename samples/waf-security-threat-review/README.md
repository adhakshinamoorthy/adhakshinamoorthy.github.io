# WAF Security Threat Review

A .NET 10 threat-review model that maps assets, actors, trust crossings, preventive controls, detection, owner, and residual risk.

## What it demonstrates

- protecting Azure workloads through zero trust, identity, data classification, network controls, secure delivery, detection, response, and continuous posture management.
- Cloud platform controls provide capabilities and signals; the workload team remains accountable for configuration, authorization, data use, code, dependencies, monitoring, and response.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/WafSecurityThreatReview
```

## Check

```powershell
dotnet run --project src/WafSecurityThreatReview -- --self-test
```

## Production boundary

Perimeter-only security leaves workload identities, control planes, software supply chain, data flows, and privileged operations exposed to misuse. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
