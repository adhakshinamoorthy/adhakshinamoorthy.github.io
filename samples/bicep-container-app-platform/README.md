# Bicep Container App Platform

A modular Bicep deployment for Azure Container Apps with Log Analytics, Application Insights, managed identity, secure parameters, encrypted ingress, health probes, scaling, environment-specific capacity, and governance tags.

## Inspect locally

```powershell
dotnet run --project tools/BicepContract -- infra
```

## Test

```powershell
dotnet test BicepContainerAppPlatform.slnx
```

## Compile and preview

```powershell
$env:EXTERNAL_API_KEY='replace-for-development'
az bicep build --file infra/main.bicep
az deployment group what-if --resource-group <resource-group> --parameters infra/main.bicepparam
```

For production, replace the example secret parameter with a Key Vault reference, use an immutable image digest, enforce policy and private networking, and deploy through workload identity with a reviewed what-if result.
