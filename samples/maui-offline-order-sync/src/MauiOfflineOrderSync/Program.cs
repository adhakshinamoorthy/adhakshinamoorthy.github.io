var store = new OfflineStore();
store.Save(new Order("ORD-7", 1, "Draft"));
store.Queue("ORD-7", "Submit", "OP-1");
var first = store.Sync("OP-1", remoteVersion: 1);
var duplicate = store.Sync("OP-1", remoteVersion: 1);
Console.WriteLine($"first={first} duplicate={duplicate} pending={store.PendingCount}");
if (args.Contains("--self-test") && (first != "Synced" || duplicate != "Already synced")) return 1;
return 0;

sealed record Order(string Id, int Version, string Status);
sealed class OfflineStore
{
    private readonly Dictionary<string, Order> orders = [];
    private readonly Dictionary<string, (string OrderId, string Action)> outbox = [];
    private readonly HashSet<string> completed = [];
    public int PendingCount => outbox.Count;
    public void Save(Order order) => orders[order.Id] = order;
    public void Queue(string orderId, string action, string operationId) => outbox[operationId] = (orderId, action);
    public string Sync(string operationId, int remoteVersion)
    {
        if (completed.Contains(operationId)) return "Already synced";
        if (!outbox.TryGetValue(operationId, out var item)) return "Missing operation";
        if (orders[item.OrderId].Version != remoteVersion) return "Conflict";
        completed.Add(operationId); outbox.Remove(operationId); return "Synced";
    }
}
