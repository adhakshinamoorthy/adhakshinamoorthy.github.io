using System.Collections.Concurrent;

namespace MinimalApisOrders.Orders;

internal sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public InMemoryOrderRepository()
    {
        var seeded = new Order(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "CUS-100",
            [new OrderLine("BOOK-1", 1)],
            DateTimeOffset.Parse("2026-01-15T09:00:00Z"));
        _orders[seeded.Id] = seeded;
    }

    public IReadOnlyList<Order> List(int limit) => _orders.Values
        .OrderByDescending(order => order.CreatedAtUtc)
        .Take(limit)
        .ToArray();

    public Order? Find(Guid id) => _orders.GetValueOrDefault(id);

    public Order Add(CreateOrderRequest request)
    {
        var order = new Order(
            Guid.NewGuid(),
            request.CustomerId.Trim(),
            request.Lines.Select(line => new OrderLine(line.Sku.Trim(), line.Quantity)).ToArray(),
            DateTimeOffset.UtcNow);
        _orders[order.Id] = order;
        return order;
    }

    public bool Delete(Guid id) => _orders.TryRemove(id, out _);
}
