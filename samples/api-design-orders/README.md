# API Design Orders

A focused .NET 10 HTTP API demonstrating stable resource contracts and operational HTTP semantics rather than exposing storage details.

## What it demonstrates

- Resource-oriented URIs and separate request/response contracts
- Bounded cursor pagination with a next cursor
- `Idempotency-Key` replay for safe create retries and conflict detection for key reuse with a different request
- Strong ETags with `If-Match` for optimistic concurrency
- `201 Created`, `409 Conflict`, `412 Precondition Failed`, and `428 Precondition Required`
- RFC-style Problem Details with correlation
- Integration tests against the complete HTTP pipeline

## Run

```powershell
dotnet run --project src/ApiDesignOrders
```

## Test

```powershell
dotnet test ApiDesignOrders.slnx
```

The in-memory store keeps the sample dependency-free. Replace it with durable storage and persist idempotency records in the same consistency boundary as the business effect before production.
