# OpenTelemetry Correlation Lab

A dependency-free .NET 10 instrumentation sample using the BCL ActivitySource and Meter APIs that OpenTelemetry consumes. It emits an order span, bounded tags, a counter, a latency histogram, and a trace identifier suitable for log correlation.

## Run
```powershell
dotnet run --project src/OtelCorrelationLab
```
## Test
```powershell
dotnet test OtelCorrelationLab.slnx
```

Production services should add the OpenTelemetry SDK, resource identity, framework instrumentation, OTLP export through a Collector, sampling, cardinality controls, redaction, retention, dashboards, and alert validation.
