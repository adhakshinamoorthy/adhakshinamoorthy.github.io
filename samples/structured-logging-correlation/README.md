# Structured Logging Correlation

A .NET 10 JSON log emitter with stable event names, trace correlation, typed properties, exception classification, and allowlist-based redaction.

## What it demonstrates

- emitting stable event templates and typed properties that correlate requests, traces, dependencies, deployments, tenants, and business outcomes without leaking sensitive data.
- Structured logs explain discrete events; metrics quantify trends and alerts, traces connect distributed work, and audits provide tamper-aware accountability.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/StructuredLoggingCorrelation
```

## Check

```powershell
dotnet run --project src/StructuredLoggingCorrelation -- --self-test
```

## Production boundary

Interpolated text, unbounded property values, inconsistent names, duplicate exception logging, or sensitive payloads make logs expensive, unqueryable, and dangerous. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
