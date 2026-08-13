using System.Diagnostics;
using OtelCorrelationLab;
using var listener = new ActivityListener { ShouldListenTo = s => s.Name == "Atlas.Orders", Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded }; ActivitySource.AddActivityListener(listener); Console.WriteLine($"Trace={new OrderProcessor().Process("order-42")}");
