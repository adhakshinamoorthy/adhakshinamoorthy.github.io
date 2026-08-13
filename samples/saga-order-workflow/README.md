# Saga Order Workflow

A runnable .NET 10 orchestrated saga state machine with durable-state-shaped transitions, message deduplication, explicit commands, and payment compensation.

## Run

```powershell
dotnet run --project src/SagaOrderWorkflow
```

## Test

```powershell
dotnet test SagaOrderWorkflow.slnx
```

Production persistence must atomically store saga state, outgoing messages, processed IDs, version, and deadlines; delivery remains at least once and compensations require their own idempotency and business semantics.
