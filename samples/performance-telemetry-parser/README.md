# Performance Telemetry Parser

A .NET 10 measurement sample comparing an allocation-heavy `string.Split` parser with a `ReadOnlySpan<char>` implementation. It separates correctness tests from BenchmarkDotNet measurement and demonstrates an optimization only after preserving behavior.

## Run the benchmark

Use a Release build without a debugger and treat results as machine-specific evidence:

```powershell
dotnet run -c Release --project benchmarks/TelemetryParser.Benchmarks -- --filter "*"
```

BenchmarkDotNet reports latency distribution and allocated bytes for both implementations. Re-run on representative hardware and inputs before making a production decision.

## Test

```powershell
dotnet test PerformanceTelemetryParser.slnx
```

Tests prove behavioral parity, bounded invalid-input handling, culture independence, and the intended allocation direction. Unit allocation checks are regression guards, not substitutes for BenchmarkDotNet.
