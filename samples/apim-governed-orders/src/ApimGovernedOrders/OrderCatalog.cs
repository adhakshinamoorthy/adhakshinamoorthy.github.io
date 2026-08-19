using System.Collections.Concurrent;

namespace ApimGovernedOrders;

public sealed record Order(Guid Id, string CustomerId, decimal Total, string Status);
public sealed record CreateOrder(string CustomerId, decimal Total);

public sealed class OrderCatalog
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Order Create(CreateOrder request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CustomerId);
        if (request.Total <= 0) throw new ArgumentOutOfRangeException(nameof(request), "Total must be positive.");

        var order = new Order(Guid.NewGuid(), request.CustomerId.Trim(), request.Total, "accepted");
        _orders[order.Id] = order;
        return order;
    }

    public Order? Find(Guid id) => _orders.GetValueOrDefault(id);
}
