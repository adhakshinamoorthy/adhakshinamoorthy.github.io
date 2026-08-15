# AWS .NET Retry Client

A .NET 10 AWS-style client policy lab that classifies throttling and server faults, applies capped jittered backoff, and refuses unsafe retries.

## What it demonstrates

- building .NET workloads on AWS with the SDK client factory, IAM roles, region-aware configuration, bounded resilience, observability, and service-specific operational design.
- The AWS SDK handles signing, serialization, endpoints, and service clients; the workload still owns IAM scope, deadlines, idempotency, retry policy, data classification, and recovery.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/AwsDotnetRetryClient
```

## Check

```powershell
dotnet run --project src/AwsDotnetRetryClient -- --self-test
```

## Production boundary

Stacking application retries over SDK retries can amplify throttling, exceed user deadlines, duplicate side effects, and hide the dependency that is actually saturated. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
