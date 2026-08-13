# Secrets Rotation

A dependency-free .NET 10 console sample that models secret retrieval, short-lived caching, versionless lookup, rotation, redacted diagnostics, and failure-safe refresh.

## Demonstrates

- Applications depending on a secret provider instead of embedded values
- Versioned secret records and versionless current-value lookup
- Bounded caching that reduces vault calls without pinning a credential forever
- Rotation without an application restart
- Metadata-only diagnostics that never print secret material
- Deterministic tests using `TimeProvider`

## Run

```powershell
dotnet run --project src/SecretsRotation
```

## Test

```powershell
dotnet test SecretsRotation.slnx
```

The in-memory vault is an educational seam. In Azure, replace it with Key Vault plus `DefaultAzureCredential`, managed identity, Azure RBAC, private networking where required, audit logging, soft delete, purge protection, and automated rotation.
