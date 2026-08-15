# Feature Flags Checkout

A .NET 10 API demonstrating safe runtime release decisions: a global kill switch, deterministic percentage rollout, stable user assignment, targeted groups, ownership, and a review deadline. The Bicep deploys Azure App Configuration with local authentication disabled and grants a workload identity the App Configuration Data Reader role.

## Run and test

```powershell
dotnet run --project src/FeatureFlagsCheckout
dotnet test FeatureFlagsCheckout.slnx
az bicep build --file infra/main.bicep
```

Call `/checkout` with `X-User-Id` to observe stable percentage assignment or add `X-Groups: staff` for a targeted rollout. In production, load definitions through Azure App Configuration and `Microsoft.FeatureManagement`, select only the required labels and keys, set an intentional refresh interval, emit evaluation telemetry without sensitive targeting attributes, test both paths, and delete the flag and dead branch after rollout.
