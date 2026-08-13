# Architecture Boundary Lab

A multi-project .NET 10 sample where architecture rules are executable fitness functions rather than conventions that drift silently.

## Dependency direction

```text
Domain <- Application <- Infrastructure <- Host
```

The application owns `IAccountStore`; infrastructure implements it; only the host composes the concrete adapter.

## What the tests enforce

- Domain cannot reference Application or Infrastructure
- Application cannot reference Infrastructure
- exported application ports use the `Store` suffix
- exported infrastructure adapters are sealed
- the composed adapter still satisfies the application behavior

## Run

```powershell
dotnet run --project src/ArchitectureBoundaryLab
```

## Test

```powershell
dotnet test ArchitectureBoundaryLab.slnx -c Release
```

These reflection-based rules need no extra architecture-test package. For larger systems, NetArchTest, ArchUnitNET, Roslyn analyzers, or custom source analyzers can express richer namespace, coupling, and accessibility policies. Introduce rules incrementally and make failures list the violating types.
