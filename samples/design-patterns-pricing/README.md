# Design Patterns Pricing

A runnable .NET 10 pricing workflow that applies patterns to concrete change pressures instead of presenting isolated class diagrams.

## Patterns demonstrated

- **Strategy** selects standard or loyalty discount behavior.
- **Adapter** translates a legacy basis-points tax gateway into the application tax contract.
- **Decorator** adds a maximum-total safety policy around any pricing service.
- **Factory** keeps composition and selection rules out of consumers.
- Tests show substitutability and verify the distinct responsibility of every pattern.

## Run

```powershell
dotnet run --project src/DesignPatternsPricing
```

## Test

```powershell
dotnet test DesignPatternsPricing.slnx
```

Patterns move complexity; they do not eliminate it. Prefer direct code until a real variation, integration mismatch, or cross-cutting policy makes the named structure easier to change and explain.
