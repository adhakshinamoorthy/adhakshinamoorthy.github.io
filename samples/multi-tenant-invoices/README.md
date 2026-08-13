# Multi-Tenant Invoices

A .NET 10 API demonstrating tenant context derived from authenticated claims, deny-by-default routing, tenant-scoped data access, non-disclosing object lookup, tenant-aware cache keys, per-tenant quotas, and negative isolation tests.

## Run

```powershell
dotnet run --project src/MultiTenantInvoices
```

For local demonstrations, send `X-User-Id` and `X-Tenant-Id` headers. The sample authentication handler turns them into claims so the rest of the app consumes trusted identity context rather than route or query tenant input. Replace that local handler with an approved identity provider in production; never trust tenant headers directly at an internet boundary.

## Test

```powershell
dotnet test MultiTenantInvoices.slnx
```

Tests prove anonymous denial, tenant-filtered lists, cross-tenant non-disclosure, cache-key separation, and quota isolation.
