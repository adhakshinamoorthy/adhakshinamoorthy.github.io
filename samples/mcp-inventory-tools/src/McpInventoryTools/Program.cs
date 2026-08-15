using System.Text.Json;
using System.Text.Json.Nodes;

var request = JsonNode.Parse(args.FirstOrDefault(a => a.StartsWith('{')) ?? "{"jsonrpc":"2.0","id":1,"method":"tools/list"}")!.AsObject();
var method = request["method"]?.GetValue<string>();
object result = method switch
{
    "initialize" => new { protocolVersion = "2025-11-25", capabilities = new { tools = new { } }, serverInfo = new { name = "inventory-tools", version = "1.0" } },
    "tools/list" => new { tools = new[] { new { name = "get_inventory", description = "Read stock for one SKU", inputSchema = new { type = "object", required = new[] { "sku" } } } } },
    "tools/call" => CallTool(request["params"] as JsonObject),
    _ => new { error = "Method not found" }
};
Console.WriteLine(JsonSerializer.Serialize(new { jsonrpc = "2.0", id = request["id"], result }));
if (args.Contains("--self-test") && method != "tools/list") return 1;
return 0;

static object CallTool(JsonObject? parameters)
{
    if (parameters?["name"]?.GetValue<string>() != "get_inventory") return new { isError = true, content = "Tool not allowed" };
    var sku = parameters["arguments"]?["sku"]?.GetValue<string>();
    return string.IsNullOrWhiteSpace(sku) || sku.Length > 32
        ? new { isError = true, content = "Invalid sku" }
        : new { isError = false, content = new { sku, available = 12 } };
}
