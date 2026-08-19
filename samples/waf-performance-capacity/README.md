# WAF Performance Capacity

A .NET 10 capacity planner using Little’s Law, headroom, autoscale lead time, and per-instance throughput to estimate safe instance count.

## What it demonstrates

- meeting workload latency and throughput targets efficiently through demand modeling, architecture, scaling, caching, data design, testing, and continuous measurement.
- Performance efficiency is an end-to-end workload property; scaling one Azure resource cannot fix chatty calls, poor queries, hot partitions, retry storms, or downstream limits.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/WafPerformanceCapacity
```

## Check

```powershell
dotnet run --project src/WafPerformanceCapacity -- --self-test
```

## Production boundary

Average latency and CPU alone hide tail latency, queueing, saturation, hot keys, connection limits, dependency throttling, and failure-time capacity collapse. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
