namespace EventDrivenOrders;

public sealed record OrderPlaced(Guid MessageId, Guid OrderId, string CustomerId, decimal Total);
public sealed record OutboxMessage(Guid Id, OrderPlaced Event, DateTimeOffset? PublishedAt);

public sealed class OrderDatabase
{
    public Dictionary<Guid, decimal> Orders { get; } = [];
    public List<OutboxMessage> Outbox { get; } = [];
    public Guid Place(string customerId, decimal total)
    {
        if (string.IsNullOrWhiteSpace(customerId) || total <= 0) throw new ArgumentException("A customer and positive total are required.");
        var orderId = Guid.NewGuid(); var messageId = Guid.NewGuid();
        Orders.Add(orderId, total);
        Outbox.Add(new(messageId, new(messageId, orderId, customerId, total), null));
        return orderId;
    }
}

public interface IEventBus { ValueTask PublishAsync(OrderPlaced message, CancellationToken cancellationToken); }
public sealed class OutboxRelay(OrderDatabase database, IEventBus bus)
{
    public async ValueTask<int> RelayAsync(CancellationToken cancellationToken = default)
    {
        var count = 0;
        for (var index = 0; index < database.Outbox.Count; index++)
        {
            var item = database.Outbox[index]; if (item.PublishedAt is not null) continue;
            await bus.PublishAsync(item.Event, cancellationToken);
            database.Outbox[index] = item with { PublishedAt = DateTimeOffset.UtcNow }; count++;
        }
        return count;
    }
}

public sealed class LoyaltyConsumer
{
    private readonly HashSet<Guid> processed = [];
    public Dictionary<string, decimal> SpendByCustomer { get; } = [];
    public bool Handle(OrderPlaced message)
    {
        if (!processed.Add(message.MessageId)) return false;
        SpendByCustomer[message.CustomerId] = SpendByCustomer.GetValueOrDefault(message.CustomerId) + message.Total;
        return true;
    }
}

public sealed class InMemoryBus(params LoyaltyConsumer[] consumers) : IEventBus
{
    public ValueTask PublishAsync(OrderPlaced message, CancellationToken cancellationToken) { cancellationToken.ThrowIfCancellationRequested(); foreach (var consumer in consumers) consumer.Handle(message); return ValueTask.CompletedTask; }
}
