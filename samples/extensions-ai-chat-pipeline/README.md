# Extensions AI Chat Pipeline

A .NET 10 provider-neutral chat client pipeline demonstrating delegating middleware, deterministic caching, timing, and a swappable local provider.

## What it demonstrates

- provider-neutral chat and embedding abstractions with composable middleware for telemetry, caching, tool use, and resilience.
- Microsoft.Extensions.AI standardizes application-facing AI contracts; provider SDKs, model behavior, data governance, and product policy remain explicit choices.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/ExtensionsAiChatPipeline
```

## Check

```powershell
dotnet run --project src/ExtensionsAiChatPipeline -- --self-test
```

## Production boundary

A provider swap can change tokenization, tool calling, safety behavior, latency, limits, or response shape even when the application interface stays stable. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
