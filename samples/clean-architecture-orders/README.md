# Clean Architecture Orders

A runnable .NET 10 example where business rules point inward: the domain has no infrastructure dependency, the application use case owns orchestration through ports, and the composition root selects adapters.

## Architecture

- `Domain`: `Order` and `OrderLine` invariants; no database, HTTP, or framework concerns.
- `Application`: input/output records, `PlaceOrderHandler`, repository and clock ports.
- `Infrastructure`: replaceable in-memory repository and system-clock adapters.
- `Program.cs`: the only composition root.
- Tests substitute ports directly and prove invalid input never reaches persistence.

## Run

```powershell
dotnet run --project src/CleanArchitectureOrders
```

## Test

```powershell
dotnet test CleanArchitectureOrders.slnx
```

This deliberately uses one assembly to keep the sample small. A production system can enforce the same dependency direction with separate projects and architecture tests; project count alone does not create clean boundaries.
