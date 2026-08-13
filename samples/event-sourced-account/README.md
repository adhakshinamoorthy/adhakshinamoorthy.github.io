# Event-Sourced Account

A runnable .NET 10 aggregate whose current state is rebuilt from an ordered event stream. It demonstrates decision-time events, replay, uncommitted changes, stream versions, and optimistic concurrency.

## Run
```powershell
dotnet run --project src/EventSourcedAccount
```
## Test
```powershell
dotnet test EventSourcedAccount.slnx
```

Production event stores also require atomic appends, global positions, metadata, schema upcasting, snapshots, projections, idempotency, retention policy, privacy handling, backup, and verified replay tooling.
