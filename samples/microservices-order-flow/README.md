# Microservices Order Flow

A .NET 10 API that makes two distributed-systems guarantees visible: an order and its outgoing event are recorded together, and the inventory consumer applies a message only once even when it is redelivered.

## Run

```powershell
dotnet run --project src/MicroservicesOrderFlow
```

Create an order with `POST /orders`, call `POST /operations/publish-next`, then inspect `GET /inventory/{sku}/reserved`. Use `GET /health` for liveness.

## Test

```powershell
dotnet test MicroservicesOrderFlow.slnx
```

This sample uses in-memory stores to keep the consistency mechanics readable. Production services must use durable service-owned databases, a transactional outbox table, a broker, durable consumer receipts, authenticated service identities, timeouts, telemetry, and independently deployable infrastructure.
