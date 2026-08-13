using System.Collections.Concurrent;

namespace TestingOrdersApi;

public sealed record CreateOrderRequest(string CustomerId, int Quantity, decimal UnitPrice);
public sealed record OrderReceipt(Guid Id, string CustomerId, int Quantity, decimal Total, DateTimeOffset CreatedAt);

public sealed class OrderService(TimeProvider timeProvider)
{
    private readonly ConcurrentDictionary<Guid, OrderReceipt> _orders = new();

    public OrderReceipt Create(CreateOrderRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CustomerId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.Quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.UnitPrice);

        var receipt = new OrderReceipt(
            Guid.NewGuid(),
            request.CustomerId,
            request.Quantity,
            request.Quantity * request.UnitPrice,
            timeProvider.GetUtcNow());

        _orders[receipt.Id] = receipt;
        return receipt;
    }

    public OrderReceipt? Find(Guid id) => _orders.GetValueOrDefault(id);
}
