# Semantic Kernel Order Agent

A credential-free .NET 10 plugin orchestrator that separates retrieval from side effects, validates tool arguments, requires approval, and records idempotent operation IDs.

## What it demonstrates

- controlled AI orchestration through small, well-described plugins, grounded context, and explicit approval before side effects.
- The kernel coordinates models and application functions; domain services still own authorization, validation, transactions, and audit.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/SemanticKernelOrderAgent
```

## Check

```powershell
dotnet run --project src/SemanticKernelOrderAgent -- --self-test
```

## Production boundary

A model can choose an irrelevant tool, invent arguments, expose sensitive context, or repeat an action after a timeout. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
