# YARP Route Policy Lab

A .NET 10 YARP policy model that performs ordered route matching, strips spoofable headers, selects healthy destinations, and rejects unsafe retries.

## What it demonstrates

- building a programmable .NET reverse proxy with explicit routes, clusters, transforms, health, load balancing, resilience, authentication boundaries, and dynamic configuration.
- YARP proxies HTTP traffic and exposes extensibility; upstream and downstream services still own business authorization, data rules, contracts, and workload-specific resilience.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/YarpRoutePolicyLab
```

## Check

```powershell
dotnet run --project src/YarpRoutePolicyLab -- --self-test
```

## Production boundary

A catch-all proxy can become an unbounded trust bridge that forwards spoofed headers, retries unsafe requests, hides unhealthy destinations, or centralizes fragile business logic. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
