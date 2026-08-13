# Testing Orders API

A .NET 10 sample that turns a testing strategy into executable layers instead of repeating one style of test everywhere.

## What it demonstrates

- deterministic unit tests with an injected `TimeProvider`
- theory-driven edge cases for domain policy
- HTTP integration tests through the real ASP.NET Core pipeline
- response-contract assertions for validation problem details
- a critical create-then-read journey with isolated in-memory state

## Run

```powershell
dotnet run --project src/TestingOrdersApi
```

Create an order:

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5000/orders -ContentType application/json -Body '{"customerId":"customer-1","quantity":2,"unitPrice":6.5}'
```

## Test

```powershell
dotnet test TestingOrdersApi.slnx -c Release
```

The suite intentionally separates deterministic business behavior from HTTP integration and contract checks. In production, add provider-real database tests and keep only a small set of end-to-end journeys through deployed infrastructure.
