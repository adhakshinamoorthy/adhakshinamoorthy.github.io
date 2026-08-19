# Azure .NET service baseline

A .NET 10 minimal API showing environment-aware Azure authentication, a reusable Azure SDK client, validated HTTPS configuration, managed identity in hosted environments, health reporting, and tests that never require a live subscription.

## Run locally

```powershell
dotnet run --project src/AzureDotNetServiceBaseline
```

Open `http://localhost:5000/` using the URL printed by ASP.NET Core. The app constructs the client but does not contact Azure until an SDK operation is made, so the baseline runs without an Azure account.

## Test

```powershell
dotnet test AzureDotNetServiceBaseline.slnx
```

## Production contract

- Local development uses `DefaultAzureCredential` without interactive-browser fallback.
- Staging and production use deterministic `ManagedIdentityCredential`.
- A user-assigned identity is selected only through `Azure:ManagedIdentityClientId`.
- The storage endpoint must be HTTPS; secrets and account keys are not configuration inputs.
- Assign the identity only the data-plane role required by the workload.

Replace the example storage URI, deploy the same artifact through infrastructure as code, and validate identity, network, telemetry, backup, scaling, and cost controls in a production-shaped environment.
