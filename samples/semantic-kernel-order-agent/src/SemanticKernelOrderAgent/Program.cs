using System.Text.Json;

var orders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["ORD-42"] = "Packed" };
var completed = new Dictionary<string, string>();

string GetStatus(string orderId) => orders.TryGetValue(orderId, out var status) ? status : "Not found";
string Cancel(string orderId, string operationId, bool approved)
{
    if (!approved) return "Approval required";
    if (completed.TryGetValue(operationId, out var prior)) return prior;
    if (!orders.ContainsKey(orderId)) return "Order not found";
    orders[orderId] = "Cancelled";
    return completed[operationId] = $"{orderId} cancelled";
}

Console.WriteLine(JsonSerializer.Serialize(new { tool = "get_order_status", result = GetStatus("ORD-42") }));
Console.WriteLine(JsonSerializer.Serialize(new { tool = "cancel_order", result = Cancel("ORD-42", "OP-100", approved: false) }));
Console.WriteLine(JsonSerializer.Serialize(new { tool = "cancel_order", result = Cancel("ORD-42", "OP-100", approved: true) }));
if (args.Contains("--self-test") && (GetStatus("ORD-42") != "Cancelled" || completed.Count != 1)) return 1;
return 0;
