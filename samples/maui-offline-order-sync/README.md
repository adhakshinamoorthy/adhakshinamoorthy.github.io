# .NET MAUI Offline Order Sync

A .NET 10 offline-order domain core suitable for a MAUI app, with a durable-style outbox, idempotent synchronization, retry state, and conflict visibility.

## What it demonstrates

- one cross-platform .NET application with native lifecycle, adaptive UI, local persistence, secure platform integration, and resilient synchronization.
- .NET MAUI shares UI and application code while platform projects, permissions, packaging, accessibility, lifecycle, and store policy remain platform-specific.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/MauiOfflineOrderSync
```

## Check

```powershell
dotnet run --project src/MauiOfflineOrderSync -- --self-test
```

## Production boundary

Assuming permanent connectivity or identical platform behavior causes data loss, frozen UI, broken navigation, permission failures, and rejected releases. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
