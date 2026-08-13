# API Security & OWASP Orders

A .NET 10 API that turns common OWASP API risks into executable controls and negative tests.

## Demonstrates

- Object-level authorization using a tenant and subject identity
- Allowlisted fields and response contracts to prevent mass assignment and excessive data exposure
- Bounded pagination, request-size limits, rate limiting, and cancellation
- Allowlisted outbound inventory destinations to reduce SSRF risk
- Safe Problem Details and security headers
- Tests for anonymous access, cross-tenant identifiers, oversized pages, over-posting, and outbound-host rejection

## Run

```powershell
dotnet run --project src/ApiSecurityOwaspOrders
```

For local calls, send `X-Subject` and `X-Tenant` headers. They are an intentionally local authentication mechanism, not a production trust boundary.

## Test

```powershell
dotnet test ApiSecurityOwaspOrders.slnx
```

In production, use a validated identity provider, durable storage, trusted proxy configuration, centralized telemetry, and deployment-level network egress controls.
