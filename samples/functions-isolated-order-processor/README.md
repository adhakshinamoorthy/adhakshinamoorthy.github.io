# Functions isolated order processor

A .NET 10 Azure Functions isolated-worker sample for queue-driven order events. It keeps business logic host-independent, validates event contracts, records idempotency receipts, uses bounded queue retries with poison-message routing, honors cancellation, and emits structured invocation context.

## Prerequisites

- .NET 10 SDK
- Azure Functions Core Tools v4
- Azurite for local queue storage

Copy `src/FunctionsIsolatedOrderProcessor/local.settings.example.json` to `local.settings.json`, start Azurite, then run:

```powershell
func start --script-root src/FunctionsIsolatedOrderProcessor
```

Add JSON such as `{"EventId":"event-1","OrderId":"order-1","Amount":42.50}` to the `orders` queue.

## Test

```powershell
dotnet test FunctionsIsolatedOrderProcessor.slnx
```

The in-memory receipt store is intentionally local-only. Production must replace it with a durable conditional insert keyed by `EventId`, use identity-based host connections where supported, monitor the `orders-poison` queue, tune concurrency from downstream capacity, and deploy the entire function app as one immutable package.

`Directory.Build.targets` filters stale runtime-specific files from the generated extension project's copy list. This guards a Windows .NET 10 SDK packaging edge case without suppressing compiler, analyzer, or NuGet warnings and has no effect on the function app assembly.
