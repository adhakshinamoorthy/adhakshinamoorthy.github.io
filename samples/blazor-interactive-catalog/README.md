# Blazor Interactive Catalog

A focused .NET 10 Blazor Web App showing how server-rendered HTML and interactive server components work together without putting business behavior in the UI.

## What this sample demonstrates

- static server prerendering followed by `InteractiveServer` hydration
- an accessible Razor component with semantic product cards, stable `@key` values, button labels, and an `aria-live` cart summary
- immutable catalog data in a singleton service and mutable per-circuit cart state in a scoped service
- focused component composition, dependency injection, routing, layouts, error handling, and static assets
- deterministic unit tests for cart behavior and isolation
- `WebApplicationFactory` tests for the real server-rendered route, interactive markers, boot script, security headers, and 404 behavior

## Project layout

```text
src/BlazorInteractiveCatalog/
  Components/                 Razor application, layout, and pages
  Models/                     Product contract
  Services/                   Catalog and scoped cart state
  wwwroot/                    CSS assets
tests/BlazorInteractiveCatalog.Tests/
  CartStateTests.cs           State and isolation tests
  CatalogPageTests.cs         HTTP integration tests
```

## Run

```powershell
cd samples/blazor-interactive-catalog
dotnet restore BlazorInteractiveCatalog.slnx
dotnet run --project src/BlazorInteractiveCatalog
```

Open the HTTPS or HTTP URL printed by the application. The catalog is useful before interactivity starts; after hydration, each **Add** button updates scoped cart state over the server circuit.

## Test

```powershell
dotnet test BlazorInteractiveCatalog.slnx
```

The integration tests prove the server-rendered application boundary. For a production application, add browser tests for hydration, clicks, reconnect behavior, keyboard navigation, focus, and the deployed reverse-proxy path.

## Production notes

- `CartState` is scoped, so interactive server gives each circuit its own instance. Never make mutable user state singleton or static.
- Hiding a button does not authorize a resource. Enforce authorization again in the trusted server service or endpoint.
- Interactive server needs WebSockets or a supported fallback, capacity testing, connection telemetry, graceful shutdown, and an intentional scale-out strategy.
- Development uses ephemeral data-protection keys so the sample runs in restricted environments. Production must persist and protect a shared key ring when multiple instances serve the app.
- Keep prerender initialization idempotent. Persist safe prerendered data when avoiding a second fetch matters.
- Test the published output behind the actual proxy, base path, TLS termination, and cache policy.
