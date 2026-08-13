# Event-Driven Orders

A runnable .NET 10 model of the transactional outbox and idempotent-consumer patterns. Order state and its outgoing event are recorded together, a relay publishes pending records, and duplicate delivery does not duplicate loyalty spend.

## Run
```powershell
dotnet run --project src/EventDrivenOrders
```
## Test
```powershell
dotnet test EventDrivenOrders.slnx
```

The in-memory bus makes delivery semantics deterministic for learning. Production code must use a durable database transaction, broker client, leases or safe competing relays, retry/backoff, dead-letter handling, observability, and reconciliation.
