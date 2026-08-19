# Orleans Shopping Cart

A .NET 10 HTTP API hosted with Orleans 10.2. Each cart ID addresses one single-threaded grain activation, so concurrent callers interact through an actor-owned state boundary instead of shared locks.

## Run

```powershell
dotnet run --project src/OrleansShoppingCart
```

Use `POST /carts/{cartId}/items/{sku}?quantity=2`, `GET /carts/{cartId}`, `DELETE /carts/{cartId}`, and `GET /health`.

## Test

```powershell
dotnet test OrleansShoppingCart.slnx
```

This learning sample deliberately uses localhost clustering and activation-only memory. Production requires durable grain storage where state must survive activation loss, a shared clustering provider, reminders or streams when appropriate, authentication at the API edge, rolling-upgrade compatibility, telemetry, placement planning, and failure testing.
