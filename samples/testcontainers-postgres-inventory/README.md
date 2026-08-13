# Testcontainers PostgreSQL Inventory

A .NET 10 integration-testing sample that provisions a real, pinned PostgreSQL container from xUnit and verifies parameterized persistence against the actual provider.

## Prerequisites

- .NET 10 SDK
- Docker Desktop or another Docker-compatible runtime with a running engine
- network access on first run to pull `postgres:16.4-alpine`

## What it demonstrates

- a typed `PostgreSqlBuilder` with an explicit image tag
- a random mapped host port and container-derived connection string
- one container shared through `IClassFixture`
- schema creation during fixture initialization
- data reset between tests without restarting the container
- real PostgreSQL constraints and parameterized Npgsql commands
- deterministic policy tests that do not require infrastructure

## Test

```powershell
dotnet test TestcontainersPostgresInventory.slnx -c Release
```

The integration tests intentionally fail when Docker is unavailable; silently substituting an in-memory provider would defeat the sample's purpose.

## Run against an existing PostgreSQL instance

```powershell
$env:POSTGRES_CONNECTION_STRING='Host=localhost;Port=5432;Database=atlas;Username=postgres;Password=postgres'
dotnet run --project src/TestcontainersPostgresInventory
```

Production test suites should also pin architecture-compatible images, isolate parallel databases or schemas, set CI resource limits, capture container logs on failure, and let Testcontainers cleanup resources even when tests are cancelled.
