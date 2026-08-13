namespace CleanArchitectureOrders.Domain
{
public sealed record OrderLine(string Sku, int Quantity, decimal UnitPrice)
{
    public static OrderLine Create(string sku, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU is required.", nameof(sku));
        if (quantity is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPrice <= 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));
        return new(sku.Trim().ToUpperInvariant(), quantity, unitPrice);
    }
}

public sealed class Order
{
    private Order(Guid id, string customerId, IReadOnlyList<OrderLine> lines)
        => (Id, CustomerId, Lines) = (id, customerId, lines);

    public Guid Id { get; }
    public string CustomerId { get; }
    public IReadOnlyList<OrderLine> Lines { get; }
    public decimal Total => Lines.Sum(line => line.Quantity * line.UnitPrice);

    public static Order Place(string customerId, IEnumerable<OrderLine> lines)
    {
        var materialized = lines.ToArray();
        if (string.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("Customer is required.", nameof(customerId));
        if (materialized.Length is 0 or > 25) throw new ArgumentException("An order needs 1 to 25 lines.", nameof(lines));
        return new(Guid.NewGuid(), customerId.Trim(), materialized);
    }
}
}

namespace CleanArchitectureOrders.Application
{
using CleanArchitectureOrders.Domain;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> FindAsync(Guid id, CancellationToken cancellationToken);
}

public interface IClock { DateTimeOffset UtcNow { get; } }
public sealed record PlaceOrderLine(string Sku, int Quantity, decimal UnitPrice);
public sealed record PlaceOrderCommand(string CustomerId, IReadOnlyList<PlaceOrderLine> Lines);
public sealed record OrderReceipt(Guid OrderId, decimal Total, DateTimeOffset AcceptedAt);

public sealed class PlaceOrderHandler(IOrderRepository repository, IClock clock)
{
    public async Task<OrderReceipt> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var order = Order.Place(command.CustomerId, command.Lines.Select(line => OrderLine.Create(line.Sku, line.Quantity, line.UnitPrice)));
        await repository.AddAsync(order, cancellationToken);
        return new(order.Id, order.Total, clock.UtcNow);
    }
}
}

namespace CleanArchitectureOrders.Infrastructure
{
using CleanArchitectureOrders.Application;
using CleanArchitectureOrders.Domain;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<Guid, Order> orders = [];
    public Task AddAsync(Order order, CancellationToken cancellationToken) { cancellationToken.ThrowIfCancellationRequested(); orders.Add(order.Id, order); return Task.CompletedTask; }
    public Task<Order?> FindAsync(Guid id, CancellationToken cancellationToken) { cancellationToken.ThrowIfCancellationRequested(); return Task.FromResult(orders.GetValueOrDefault(id)); }
}

public sealed class SystemClock : IClock { public DateTimeOffset UtcNow => DateTimeOffset.UtcNow; }
}
