# DDD Subscriptions

A runnable .NET 10 domain model centered on the Subscription aggregate. It demonstrates a bounded consistency boundary, behavior-rich entities, validated value objects, ubiquitous business language, and domain events that record facts without performing infrastructure work.

## Model

- `Subscription` is the aggregate root and the only entry point for plan changes and cancellation.
- `CustomerId` and `Plan` are value objects that cannot represent invalid values.
- Invariants prevent duplicate plan changes and mutation after cancellation.
- Domain events describe committed business intent; an application layer would persist and dispatch them reliably.
- Tests exercise the aggregate directly with no database, host, mocks, or framework container.

## Run

```powershell
dotnet run --project src/DddSubscriptions
```

## Test

```powershell
dotnet test DddSubscriptions.slnx
```

This sample focuses on the domain boundary. Production persistence should rehydrate the aggregate, apply optimistic concurrency, save state and outbox records atomically, then publish integration events separately.
