using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ApiDesignOrders.Orders;

internal sealed record Order(Guid Id, string CustomerId, IReadOnlyList<OrderLineRequest> Lines, long Version)
{
    public string ETag => $"\"{Version}\"";
}

internal enum CreateOrderStatus { Created, Replayed, Conflict }
internal sealed record CreateOrderResult(CreateOrderStatus Status, Order? Order);
internal enum ReplaceOrderStatus { Replaced, NotFound, PreconditionFailed }
internal sealed record ReplaceOrderResult(ReplaceOrderStatus Status, Order? Order);

internal sealed class OrderStore
{
    private readonly object _gate = new();
    private readonly List<Order> _orders = [];
    private readonly Dictionary<string, IdempotencyRecord> _idempotency = new(StringComparer.Ordinal);

    public OrderStore()
    {
        for (var index = 1; index <= 3; index++)
        {
            _orders.Add(new Order(
                Guid.Parse($"00000000-0000-0000-0000-{index:D12}"),
                $"CUS-{index:000}",
                [new OrderLineRequest($"SKU-{index}", index)],
                1));
        }
    }

    public OrderPage List(int offset, int limit)
    {
        lock (_gate)
        {
            var items = _orders.Skip(offset).Take(limit).Select(OrderResponse.From).ToArray();
            var nextOffset = offset + items.Length;
            var next = nextOffset < _orders.Count ? OrderCursor.Format(nextOffset) : null;
            return new OrderPage(items, next);
        }
    }

    public Order? Find(Guid id)
    {
        lock (_gate) return _orders.FirstOrDefault(order => order.Id == id);
    }

    public CreateOrderResult Create(string key, CreateOrderRequest request)
    {
        var fingerprint = Fingerprint(request);
        lock (_gate)
        {
            if (_idempotency.TryGetValue(key, out var existing))
            {
                return existing.Fingerprint == fingerprint
                    ? new(CreateOrderStatus.Replayed, existing.Order)
                    : new(CreateOrderStatus.Conflict, null);
            }

            var order = new Order(Guid.NewGuid(), request.CustomerId.Trim(), request.Lines.ToArray(), 1);
            _orders.Add(order);
            _idempotency[key] = new(fingerprint, order);
            return new(CreateOrderStatus.Created, order);
        }
    }

    public ReplaceOrderResult Replace(Guid id, string ifMatch, ReplaceOrderRequest request)
    {
        lock (_gate)
        {
            var index = _orders.FindIndex(order => order.Id == id);
            if (index < 0) return new(ReplaceOrderStatus.NotFound, null);
            var current = _orders[index];
            if (!string.Equals(current.ETag, ifMatch, StringComparison.Ordinal))
                return new(ReplaceOrderStatus.PreconditionFailed, null);
            var replaced = current with { CustomerId = request.CustomerId.Trim(), Lines = request.Lines.ToArray(), Version = current.Version + 1 };
            _orders[index] = replaced;
            return new(ReplaceOrderStatus.Replaced, replaced);
        }
    }

    private static string Fingerprint(CreateOrderRequest request) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request))));

    private sealed record IdempotencyRecord(string Fingerprint, Order Order);
}

internal static class OrderCursor
{
    public static bool TryParse(string? value, out int offset)
    {
        offset = 0;
        if (string.IsNullOrWhiteSpace(value)) return true;
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(value));
            return int.TryParse(decoded, out offset) && offset >= 0;
        }
        catch (FormatException) { return false; }
    }

    public static string Format(int offset) => Convert.ToBase64String(Encoding.UTF8.GetBytes(offset.ToString()));
}
