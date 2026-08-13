using Telemetry.Generated;

namespace TelemetryDemo;

[GenerateTelemetry]
public sealed partial record OrderCompleted(
    Guid OrderId,
    string CustomerId,
    decimal Total,
    DateTimeOffset OccurredAt);
