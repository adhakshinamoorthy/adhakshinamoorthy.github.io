# Minimal APIs Orders

A focused .NET 10 API that shows how compact endpoints can still have explicit contracts, feature organization, validation, authorization, OpenAPI metadata, rate limiting, error handling, and integration tests.

## What it demonstrates

- A cohesive `/api/orders` route group with stable endpoint names and tags
- Request binding from routes, queries, headers, JSON bodies, DI, and cancellation
- `TypedResults` and union result types for compile-time response contracts
- An endpoint filter that validates create-order requests before the handler runs
- Policy-based authorization backed by a deliberately local-only API-key scheme
- First-party OpenAPI generation, Problem Details, security headers, and rate limiting
- Thin handlers delegating state and business work to an injected repository
- End-to-end tests through `WebApplicationFactory`

## Run

```powershell
dotnet run --project src/MinimalApisOrders
```

Then call the API at the address printed by ASP.NET Core:

```powershell
curl http://localhost:5000/api/orders
curl -H "X-Api-Key: local-development-key" -H "Content-Type: application/json" `
  -d '{"customerId":"CUS-200","lines":[{"sku":"BOOK-1","quantity":2}]}' `
  http://localhost:5000/api/orders
```

The OpenAPI document is available at `/openapi/v1.json` in Development.

## Test

```powershell
dotnet test MinimalApisOrders.slnx
```

## Production note

The API-key handler exists only to make authorization behavior runnable without an external identity provider. Replace it with validated OAuth 2.0/OIDC bearer tokens or another approved production scheme. Store credentials outside source control, protect OpenAPI exposure, terminate TLS at a trusted boundary, and use a durable repository before production.
