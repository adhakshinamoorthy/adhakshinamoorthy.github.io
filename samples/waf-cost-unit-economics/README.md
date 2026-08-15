# WAF Cost Unit Economics

A .NET 10 unit-economics calculator that allocates shared Azure cost, calculates cost per order, detects idle spend, and flags budget variance.

## What it demonstrates

- aligning Azure workload cost with business value using ownership, allocation, unit economics, demand shaping, rate optimization, waste removal, and financial guardrails.
- Cost optimization maximizes value for required quality levels; it is not indiscriminate spend reduction or a finance-only responsibility.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/WafCostUnitEconomics
```

## Check

```powershell
dotnet run --project src/WafCostUnitEconomics -- --self-test
```

## Production boundary

Monthly totals without workload, environment, owner, and business-unit context hide idle waste, scaling inefficiency, regressions, and expensive architecture choices. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
