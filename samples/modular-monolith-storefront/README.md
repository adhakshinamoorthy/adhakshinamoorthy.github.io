# Modular Monolith Storefront

A runnable .NET 10 storefront with one deployment and explicit module contracts. The Orders module cannot reach Catalog state directly; it uses narrow query and command contracts and publishes an integration message after a successful reservation.

## Boundaries

- `CatalogModule` owns product and inventory state.
- `OrdersModule` owns order placement and depends only on `ICatalogQueries`, `IInventoryCommands`, and `IOrderEvents`.
- `OrderAccepted` is an explicit integration contract, not a leaked internal entity.
- `Program.cs` composes modules in-process, retaining simple operations and transactions.
- Tests prove success, failure atomicity, and substitutability of public module contracts.

## Run

```powershell
dotnet run --project src/ModularMonolithStorefront
```

## Test

```powershell
dotnet test ModularMonolithStorefront.slnx
```

For a larger system, put module APIs and internals in separate projects, prohibit internal project references with architecture tests, and give each module schema ownership even when one database server is shared.
