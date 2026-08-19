# MCP Inventory Tools

A .NET 10 line-oriented JSON-RPC MCP teaching server that implements initialize, tools/list, and a schema-validated inventory tool without external services.

## What it demonstrates

- interoperable discovery and invocation of tools, resources, and prompts over a capability-negotiated JSON-RPC protocol.
- MCP describes how a host, client, and server exchange capabilities; the server remains responsible for identity, authorization, validation, rate limits, and safe tool semantics.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/McpInventoryTools
```

## Check

```powershell
dotnet run --project src/McpInventoryTools -- --self-test
```

## Production boundary

An overpowered tool, confused-deputy flow, untrusted server, prompt injection, or unbounded result can turn useful model context into a security incident. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
