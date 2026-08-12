using System.Text.Json;
using TelemetryDemo;

var completed = new OrderCompleted(
    Guid.Parse("10000000-0000-0000-0000-000000000001"),
    "customer-42",
    149.95m,
    DateTimeOffset.Parse("2026-08-12T10:00:00Z"));

Console.WriteLine(JsonSerializer.Serialize(completed.ToTelemetry(), new JsonSerializerOptions
{
    WriteIndented = true
}));
