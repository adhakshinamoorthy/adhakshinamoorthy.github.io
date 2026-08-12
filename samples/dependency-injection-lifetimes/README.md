# Dependency Injection Lifetimes

A focused .NET 10 console sample showing how the built-in container composes an application, owns service lifetimes, validates configuration, resolves keyed implementations, and disposes scopes.

## What this sample teaches

- A feature registration extension that keeps the composition root readable
- Singleton, scoped, and transient lifetimes with observable instance identifiers
- Explicit scope creation for each logical operation in a console or worker process
- Strongly typed options with startup validation
- Keyed email and SMS implementations without conditional logic in business code
- Constructor injection instead of service-location calls in application services
- `ValidateOnBuild` and `ValidateScopes` container checks
- Deterministic xUnit tests for identity, isolation, keyed resolution, behavior, and disposal

## Architecture

```text
Host / composition root
        |
        v
FulfillmentRunner -- creates one scope per order
        |
        v
FulfillmentProcessor (scoped)
   |          |             |
repository  options   keyed notification channels
   |
operation scope + transient activity + singleton application identity
```

## Run locally

From this folder:

```powershell
dotnet restore DependencyInjectionLifetimes.slnx
dotnet run --project src/DependencyInjectionLifetimes
```

The output processes two orders in separate scopes. Each line exposes the stable application identifier, the per-scope identifier, and a per-resolution activity identifier so lifetime behavior is visible rather than theoretical.

Override validated configuration through environment variables:

```powershell
$env:Fulfillment__MaximumQuantity = "25"
dotnet run --project src/DependencyInjectionLifetimes
```

## Run tests

```powershell
dotnet test DependencyInjectionLifetimes.slnx --configuration Release
```

The tests prove singleton reuse, scoped isolation, transient creation, keyed channel selection, behavior across two operation scopes, invalid-option rejection, and scoped-service disposal.

## Deliberate boundaries

The repository is in-memory and the channels write deterministic messages instead of contacting external systems. Database lifetime rules belong in the EF Core and Dapper samples; retry and network behavior belong in HTTP or messaging samples. This keeps every included line focused on container composition and ownership.
