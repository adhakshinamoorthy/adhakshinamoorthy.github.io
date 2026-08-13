# Authentication & Authorization Documents

A .NET 10 API demonstrating the difference between identity, permission, and resource ownership. A deliberately local-only header scheme makes the sample runnable without an identity provider; production APIs should replace it with fully validated OAuth/OIDC access tokens.

## Demonstrates

- Authentication scheme, handler, challenge, and forbid behavior
- A fallback policy that requires identity by default
- Named scope policy for document creation
- Resource-based authorization for owners and administrators
- Minimal response contracts that avoid leaking document existence
- Negative integration tests for anonymous, wrong-scope, non-owner, owner, and administrator callers

## Run

```powershell
dotnet run --project src/AuthenticationAuthorizationDocuments
```

Send `X-User`, `X-Scope`, and optional `X-Role: admin` headers for local testing.

## Test

```powershell
dotnet test AuthenticationAuthorizationDocuments.slnx
```

## Production boundary

Replace `DevelopmentHeaderAuthenticationHandler` with an approved identity provider and JWT bearer or cookie configuration. Validate issuer, audience, signature, lifetime, and allowed algorithms. Never accept identity claims directly from public request headers.
