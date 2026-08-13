using System.Diagnostics;
using System.Diagnostics.Metrics;
namespace OtelCorrelationLab;

public static class Telemetry { public static readonly ActivitySource Activities = new("Atlas.Orders"); public static readonly Meter Meter = new("Atlas.Orders"); public static readonly Counter<long> Processed = Meter.CreateCounter<long>("orders.processed"); public static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("orders.duration", "ms"); }
public sealed class OrderProcessor { public string Process(string orderId) { var start = Stopwatch.GetTimestamp(); using var activity = Telemetry.Activities.StartActivity("order.process", ActivityKind.Internal); activity?.SetTag("order.id", orderId); try { Telemetry.Processed.Add(1, new KeyValuePair<string, object?>("outcome", "success")); activity?.SetStatus(ActivityStatusCode.Ok); return activity?.TraceId.ToString() ?? "unsampled"; } finally { Telemetry.Duration.Record(Stopwatch.GetElapsedTime(start).TotalMilliseconds); } } }
