# EF Core Order Management

A focused .NET 10 sample for learning Entity Framework Core as a database access technology. It uses a small order domain to demonstrate model configuration, owned types, relationships, constraints, query projection, no-tracking reads, explicit transactions, optimistic concurrency, migrations, SQLite tests, and an opt-in PostgreSQL provider test with Testcontainers.

This is deliberately a console application rather than another HTTP API. The boundary under test is EF Core and the database.

## Prerequisites

- .NET 10 SDK
- Docker Desktop only for the optional PostgreSQL test

## Run with SQLite

```powershell
dotnet tool restore
dotnet restore
dotnet run --project src/EfCoreOrderManagement
```

The application creates `artifacts/orders.db`, applies the checked-in migration, inserts deterministic seed data, executes a projected read-only query, and prints both the result and generated SQL.

Choose another database file when you want an isolated run:

```powershell
dotnet run --project src/EfCoreOrderManagement -- --database artifacts/demo.db
```

## Run the deterministic tests

```powershell
dotnet test EfCoreOrderManagement.slnx
```

The default suite uses real SQLite database files and verifies:

- The migration creates the expected relational schema
- Projection returns the correct total without tracking entities
- The unique normalized-email index rejects duplicates
- A concurrency token rejects a stale update

## Run the PostgreSQL provider test

Start Docker Desktop, then opt into the container-backed test:

```powershell
$env:EFCORE_RUN_POSTGRES_TESTS = "1"
dotnet test EfCoreOrderManagement.slnx
Remove-Item Env:EFCORE_RUN_POSTGRES_TESTS
```

Testcontainers starts an isolated `postgres:18-alpine` container, creates the schema from the model, runs the seed workflow and projection, then removes the container. The checked-in migration is SQLite-specific; production systems using multiple providers should maintain and test separate provider-specific migration sets.

## Work with migrations

Create a migration after changing the model:

```powershell
dotnet tool run dotnet-ef migrations add DescribeTheChange `
  --project src/EfCoreOrderManagement `
  --startup-project src/EfCoreOrderManagement `
  --output-dir Persistence/Migrations
```

Review the generated operations before committing them. Generate a deployment script rather than silently migrating every production instance at startup:

```powershell
dotnet tool run dotnet-ef migrations script `
  --project src/EfCoreOrderManagement `
  --startup-project src/EfCoreOrderManagement `
  --output artifacts/migrate.sql
```

SQLite does not support EF Core's idempotent migration-script mode. Providers with the required procedural capabilities can add `--idempotent`; otherwise use a migration bundle or a deployment system that tracks the applied migration explicitly.

## Structure

```text
src/EfCoreOrderManagement/
  Application/       Bounded projections, transaction workflow, seed data
  Domain/            Customer and Order aggregates with invariants
  Persistence/       DbContext, Fluent API mappings, migrations
tests/EfCoreOrderManagement.Tests/
  SQLite schema, query, constraint, and concurrency tests
  Opt-in PostgreSQL/Testcontainers provider test
```

## Important decisions

- `DbContext` instances are short-lived and never shared across concurrent work.
- Domain objects protect invariants; Fluent API owns persistence details.
- Read models are projected in SQL and use `AsNoTracking`.
- Database constraints back up application validation.
- The `Version` property is an optimistic concurrency token and changes with state transitions.
- SQLite keeps the default workflow fast; PostgreSQL proves important provider behavior when explicitly requested.
- The SQLite native bundle is pinned directly to a current audited release instead of accepting the provider's vulnerable transitive minimum.
- Migrations are checked in, reviewed, tested, and intended to be deployed as an operational step.
