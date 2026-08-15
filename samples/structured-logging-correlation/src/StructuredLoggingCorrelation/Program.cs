using System.Text.Json;

var traceId = Guid.NewGuid().ToString("N");
var input = new Dictionary<string, object?> { ["order.id"] = "ORD-42", ["customer.email"] = "person@example.com", ["duration.ms"] = 37 };
var allowed = new HashSet<string> { "order.id", "duration.ms" };
var properties = input.ToDictionary(x => x.Key, x => allowed.Contains(x.Key) ? x.Value : "[REDACTED]");
var entry = new { timestamp = DateTimeOffset.UtcNow, level = "Information", eventName = "OrderAccepted", traceId, properties };
Console.WriteLine(JsonSerializer.Serialize(entry));
if (args.Contains("--self-test") && !Equals(properties["customer.email"], "[REDACTED]")) return 1;
return 0;
