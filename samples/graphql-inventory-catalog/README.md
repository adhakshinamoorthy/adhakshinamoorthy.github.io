# GraphQL Inventory Catalog

A .NET 10 Hot Chocolate sample demonstrating a strongly typed schema, bounded cursor paging, field selection, policy-protected mutation, domain payload errors, schema inspection, and HTTP integration tests.

## Run

```powershell
dotnet run --project src/GraphQlInventoryCatalog
```

Open `/graphql` in Development. Queries are public for the sample; mutations require local headers `X-User-Id` and `X-Permission: inventory.write`. Replace this demonstration identity scheme with your approved provider in production.

## Test

```powershell
dotnet test GraphQlInventoryCatalog.slnx
```

Tests execute real GraphQL-over-HTTP requests for selection, paging, domain errors, and mutation authorization.
