# C# Language Workbench

A focused .NET 10 console application that evaluates orders while demonstrating how modern C# language features work together in maintainable application code.

## What this sample demonstrates

- C# 14 and nullable reference analysis with warnings treated as errors
- records and `readonly record struct` value objects
- primary constructors, collection expressions, and immutable-style domain contracts
- generic collections and explicit `IReadOnlyList<T>` boundaries
- property, relational, and guarded pattern matching
- C# 14 extension members, including an extension property
- LINQ aggregation, ordering, materialization, and deterministic tie-breaking
- asynchronous streams with `IAsyncEnumerable<T>`, `await foreach`, and cancellation
- precise validation and exception boundaries
- deterministic xUnit tests for domain rules, async flow, and edge cases

## Run

From this directory:

```powershell
dotnet restore CSharpLanguageWorkbench.slnx
dotnet run --project src/CSharpLanguageWorkbench
```

Expected output:

```text
ada@example.com: USD 1250.00 -> ManualReview (promotion: True)
grace@example.com: USD 90.00 -> Fulfil (promotion: False)
```

## Test

```powershell
dotnet test CSharpLanguageWorkbench.slnx --configuration Release
```

The sample has no database, network, secret, or external-service dependency. Change the threshold or input orders in `Program.cs` to explore pattern matching, value semantics, LINQ, and async streaming behavior.
