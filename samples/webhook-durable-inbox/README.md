# Webhook Durable Inbox

A .NET 10 receiver demonstrating how to verify raw webhook deliveries, acknowledge quickly, persist before acknowledgement, and process duplicates safely in the background.

## What it demonstrates

- HMAC-SHA256 verification over the exact raw UTF-8 request bytes
- Constant-time signature comparison
- Required bounded delivery identifiers and event types
- A file-backed inbox persisted atomically before returning `202 Accepted`
- Duplicate delivery detection using the provider delivery ID
- A background worker that resumes pending deliveries after restart
- Processing state and attempts exposed through a sample-only status endpoint
- Integration tests for valid, invalid, duplicate, and Unicode deliveries

## Configure and run

Set a local-only secret outside source control:

```powershell
$env:WebhookSecret = "local-test-only-secret"
dotnet run --project src/WebhookDurableInbox
```

Generate `X-Signature-256` as `sha256=` followed by the lowercase HMAC-SHA256 hex digest of the exact request body. Also send `X-Delivery-Id` and `X-Event-Type`.

## Test

```powershell
dotnet test WebhookDurableInbox.slnx
```

The file store makes durable ordering visible without infrastructure. Production systems should use a transactional database inbox or durable queue, protect the status surface, retrieve signing secrets from a managed store, and coordinate the inbox record with idempotent business effects.
