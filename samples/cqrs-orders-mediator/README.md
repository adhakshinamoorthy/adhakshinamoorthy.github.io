# CQRS Orders Mediator

A dependency-free .NET 10 teaching implementation of the same request/handler and pipeline concepts commonly used with MediatR. Commands mutate an order write model, queries read a purpose-built projection, and validation plus idempotency wrap command handling as ordered behaviors.

## What it demonstrates

- Different command and query contracts instead of one CRUD service model.
- A mediator dispatch boundary and ordered pipeline behaviors.
- Validation before side effects and idempotent command replay.
- A write store separated from the `OrderView` read projection.
- Tests for command/query flow, failure, missing reads, and duplicate delivery.

## Run

```powershell
dotnet run --project src/CqrsOrdersMediator
```

## Test

```powershell
dotnet test CqrsOrdersMediator.slnx
```

Production systems can replace the small dispatcher with MediatR. CQRS does not require separate databases or eventual consistency; start with logical model separation, then add asynchronous projections only when scale or ownership demands it.
