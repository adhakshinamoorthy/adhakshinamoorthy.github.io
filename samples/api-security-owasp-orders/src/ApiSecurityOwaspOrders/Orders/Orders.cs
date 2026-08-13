using System.Collections.Concurrent;

namespace ApiSecurityOwaspOrders.Orders;

internal sealed record Order(Guid Id, string Tenant, string Subject, IReadOnlyList<OrderLine> Lines, decimal InternalRiskScore);
internal sealed record OrderLine(string Sku, int Quantity);
internal sealed record CreateOrderRequest(IReadOnlyList<CreateOrderLineRequest> Lines);
internal sealed record CreateOrderLineRequest(string Sku, int Quantity);
internal sealed record OrderResponse(Guid Id, IReadOnlyList<OrderLine> Lines)
{
    public static OrderResponse From(Order order) => new(order.Id, order.Lines);
}

internal interface IOrderRepository
{
    IReadOnlyList<Order> List(string tenant, int limit);
    Order? Find(Guid id);
    Order Add(string tenant, string subject, CreateOrderRequest request);
}

internal sealed class InMemoryOrderRepository : IOrderRepository
{
    public static readonly Guid AliceOrderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();
    public InMemoryOrderRepository() => _orders[AliceOrderId] = new(AliceOrderId, "north", "alice", [new("BOOK-1", 1)], 0.12m);
    public IReadOnlyList<Order> List(string tenant, int limit) => _orders.Values.Where(x => x.Tenant == tenant).Take(limit).ToArray();
    public Order? Find(Guid id) => _orders.GetValueOrDefault(id);
    public Order Add(string tenant, string subject, CreateOrderRequest request)
    {
        var order = new Order(Guid.NewGuid(), tenant, subject, request.Lines.Select(x => new OrderLine(x.Sku, x.Quantity)).ToArray(), 0m);
        _orders[order.Id] = order;
        return order;
    }
}

internal sealed class InventoryDestinationPolicy(IEnumerable<string> allowedHosts)
{
    private readonly HashSet<string> _allowedHosts = new(allowedHosts, StringComparer.OrdinalIgnoreCase);
    public bool IsAllowed(string destination) => Uri.TryCreate(destination, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps && _allowedHosts.Contains(uri.Host);
}
