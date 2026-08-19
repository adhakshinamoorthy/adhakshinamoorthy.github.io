# APIM Governed Orders

A .NET 10 orders backend plus source-controlled Azure API Management assets. The API runs locally with no cloud account; the policy demonstrates Entra JWT validation, per-caller rate limiting, correlation, bounded backend forwarding, response hardening, and a safe error contract.

## Run locally

```powershell
dotnet run --project src/ApimGovernedOrders
```

Create an order with `POST /orders` using `{"customerId":"customer-42","total":125.50}`, then read it from the returned location. APIM applies the external security and traffic policy when deployed; the backend still owns validation and domain authorization.

## Test and deployment

```powershell
dotnet test ApimGovernedOrders.slnx
az bicep build --file infra/main.bicep
```

Supply real publisher values, a reachable HTTPS backend URL, named values, certificates, identities, diagnostics, and a production tier through your deployment pipeline. Apply `infra/policies/orders-api.xml` at API scope after replacing named values. Never put secrets in policy or parameter files.

For production, select tier, zones, regions, networking, and capacity from measured requirements; use managed identity to the backend and Key Vault-backed named values; promote immutable revisions; monitor gateway capacity, latency, policy failures, backend errors, certificates, and quotas; and retain backend authorization.
