# OpenAPI & Scalar Catalog

A .NET 10 catalog API whose generated OpenAPI document is treated as a tested contract and rendered with a development-only Scalar reference UI.

## What it demonstrates

- First-party ASP.NET Core OpenAPI 3.1 generation
- Stable operation IDs, tags, summaries, descriptions, typed results, and declared validation responses
- Separate request and response models that produce useful schemas
- Scalar consuming `/openapi/v1.json` without generating the contract itself
- Development-only document and interactive UI exposure
- Contract tests for paths, operation IDs, response codes, schemas, and the Scalar route

## Run

```powershell
dotnet run --project src/OpenApiScalarCatalog
```

In Development, open `/openapi/v1.json` for the portable contract or `/scalar/v1` for the interactive reference.

## Test

```powershell
dotnet test OpenApiScalarCatalog.slnx
```

The sample pins a patched `Microsoft.OpenApi` version because contract tooling is part of the application supply chain and must be audited like runtime dependencies.
