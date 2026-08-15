# Key Vault Rotation API

A .NET 10 local model of the Key Vault secret-version lifecycle: create a new version, activate it deliberately, cache only for a bounded period, invalidate during rollout, and expose only version/fingerprint metadata. Secret values never appear in API responses. The Bicep deploys a recovery-protected, RBAC-enabled vault and grants a workload identity the Key Vault Secrets User role.

## Run and test

```powershell
dotnet run --project src/KeyVaultRotationApi
dotnet test KeyVaultRotationApi.slnx
az bicep build --file infra/main.bicep
```

Create `Payments--ApiKey` versions through `/admin/secrets/{name}/versions`, activate a version, then inspect `/payments/credential`. In production, replace `InMemoryVersionedSecretStore` with `SecretClient(new Uri(vaultUri), new DefaultAzureCredential())`; authorize the administrative endpoints separately or remove them, use overlapping credentials during rotation, and never log resolved values.
