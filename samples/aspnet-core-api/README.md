# ASP.NET Core API gold-standard sample

This focused .NET 10 sample turns the ASP.NET Core guide into a runnable product-catalog API. It demonstrates the framework features a developer needs to understand before moving to databases, authentication, messaging, or cloud deployment.

## What this sample teaches

- Application startup and dependency registration in `Program.cs`
- Intentional middleware ordering
- Minimal API route groups and feature-focused endpoint organization
- Dependency injection through a repository boundary
- Consistent validation and RFC 7807 problem details
- Request cancellation propagation
- Correlation IDs and structured logging scopes
- Fixed-window rate limiting
- Separate liveness and readiness endpoints
- End-to-end tests with `WebApplicationFactory`

## Architecture

```text
HTTP client
    |
    v
Exception handling -> Correlation ID -> Rate limiting
    |
    v
Product endpoints -> IProductRepository -> In-memory store
    |
    v
JSON response / ProblemDetails
```

The in-memory repository is deliberate: it keeps this sample about ASP.NET Core. Database persistence belongs in the dedicated Entity Framework Core and Dapper samples.

## Run locally

From this folder:

```powershell
dotnet restore AspNetCoreApi.slnx
dotnet run --project src/AspNetCoreApi.Api
```

The development profile listens on `http://localhost:5085`. Open:

- `http://localhost:5085/`
- `http://localhost:5085/api/products`
- `http://localhost:5085/health/live`
- `http://localhost:5085/health/ready`

Use [`src/AspNetCoreApi.Api/AspNetCoreApi.Api.http`](src/AspNetCoreApi.Api/AspNetCoreApi.Api.http) to exercise the complete workflow from an HTTP-client-enabled editor.

## Run tests

```powershell
dotnet test AspNetCoreApi.slnx
```

The tests verify successful reads and writes, validation problem details, and correlation-header propagation through the real HTTP pipeline.

## Endpoint contract

| Method | Route | Purpose | Expected response |
|---|---|---|---|
| `GET` | `/` | Discover the sample | `200 OK` |
| `GET` | `/api/products` | List products | `200 OK` |
| `GET` | `/api/products/{id}` | Find one product | `200 OK` or `404 Not Found` |
| `POST` | `/api/products` | Create a product | `201 Created` or `400 Bad Request` |
| `GET` | `/health/live` | Confirm the process is alive | `200 OK` |
| `GET` | `/health/ready` | Confirm dependencies are ready | `200 OK` |

## Production evolution

Before using this shape in a production service:

1. Replace the in-memory repository with durable storage and migrations.
2. Add authentication and policy-based authorization.
3. Put rate-limit counters in shared infrastructure when running multiple instances.
4. Export logs, metrics, and traces through OpenTelemetry.
5. Restrict health-check details and define real dependency checks.
6. Add API versioning and an OpenAPI contract when external clients depend on the service.
7. Build and scan the container in CI, then deploy it as a non-root workload.

## Deliberate boundaries

This sample does not pretend to be a complete commerce system. It excludes persistence, user accounts, distributed messaging, and cloud resources so every included line supports the ASP.NET Core learning objective.
