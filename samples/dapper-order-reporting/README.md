# Dapper Order Reporting

A focused .NET 10 sample for learning Dapper as an explicit SQL data-access tool. It uses a small order-reporting database to demonstrate parameter binding, bounded projections, allowlisted dynamic sorting, multi-mapping, multiple result sets, connection ownership, cancellation, database constraints, and atomic transactions.

This is deliberately a console application rather than another HTTP API. The boundary under test is Dapper, ADO.NET, and the relational database.

## Prerequisites

- .NET 10 SDK

No database server or Docker installation is required. The sample uses a real SQLite database through `Microsoft.Data.Sqlite`.

## Run the sample

```powershell
dotnet restore
dotnet run --project src/DapperOrderReporting
```

The application creates `artifacts/dapper-orders.db`, applies an idempotent SQL schema, inserts deterministic seed data, executes a filtered and bounded report, and reads a two-result dashboard in one round trip.

Choose another database file for an isolated run:

```powershell
dotnet run --project src/DapperOrderReporting -- --database artifacts/demo.db
```

## Run the integration tests

```powershell
dotnet test DapperOrderReporting.slnx
```

Every test creates a private SQLite database file and verifies actual SQL behavior:

- Filtering, aggregation, projection, and bounded pagination
- Stable ordering across pages
- One-to-many multi-mapping
- `QueryMultiple` dashboard results
- Atomic commit of an order and all its lines
- Rollback when a database uniqueness constraint rejects a line

## Where each Dapper concept lives

| Concept | Implementation |
| --- | --- |
| Parameterized queries and cancellation | `Persistence/OrderQueries.cs` |
| Safe dynamic SQL | The `OrderSort` enum maps only to trusted `ORDER BY` fragments |
| Projection and pagination | `SearchAsync` selects only the report columns and caps page size at 100 |
| Multi-mapping | `GetWithLinesAsync` folds joined rows into one order with many lines |
| Multiple result sets | `GetDashboardAsync` reads status counts and top customers with `QueryMultipleAsync` |
| Transaction ownership | `Persistence/OrderWriter.cs` passes the same transaction to every command |
| Relational constraints and indexes | `Persistence/DatabaseInitializer.cs` |
| Provider-realistic tests | `tests/DapperOrderReporting.Tests` |

## Production notes

- Treat SQL text, mappings, indexes, and query plans as production code.
- Parameterize values. Identifiers, directions, and SQL fragments cannot be parameters, so select them from allowlisted constants.
- Open connections late and dispose them promptly. ADO.NET pooling makes this inexpensive for server databases.
- Pass the same connection and transaction to every statement that must be atomic.
- Keep schema evolution in a real migration tool or reviewed deployment scripts. Dapper does not manage migrations.
- Test important queries against the production database engine because SQLite, PostgreSQL, and SQL Server differ in syntax, types, locking, and plans.
- Store money as integer minor units in this sample to avoid floating-point ambiguity. Choose a database type and domain representation that match your production requirements.

## Package versions

- Dapper 2.1.79
- Microsoft.Data.Sqlite 10.0.10
- SQLitePCLRaw.bundle_e_sqlite3 3.0.5
- xUnit 2.9.3
