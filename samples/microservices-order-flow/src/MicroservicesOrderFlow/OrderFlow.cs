namespace MicroservicesOrderFlow;

public sealed record PlaceOrder(string CustomerId, IReadOnlyList<OrderLine> Lines);
public sealed record OrderLine(string Sku, int Quantity);
public sealed record OrderPlaced(Guid MessageId, Guid OrderId, string CustomerId, IReadOnlyList<OrderLine> Lines);
public sealed record OrderView(Guid Id, string CustomerId, string Status, IReadOnlyList<OrderLine> Lines);

public sealed class OrderService
{
    private readonly object gate = new();
    private readonly Dictionary<Guid, OrderView> orders = [];
    private readonly Queue<OrderPlaced> outbox = [];

    public OrderView Place(PlaceOrder command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command.CustomerId);
        if (command.Lines.Count is 0 or > 25 || command.Lines.Any(x => string.IsNullOrWhiteSpace(x.Sku) || x.Quantity is < 1 or > 100))
            throw new ArgumentException("Orders require 1-25 valid lines with quantities from 1-100.", nameof(command));

        lock (gate)
        {
            var order = new OrderView(Guid.NewGuid(), command.CustomerId, "Placed", command.Lines);
            orders.Add(order.Id, order);
            outbox.Enqueue(new OrderPlaced(Guid.NewGuid(), order.Id, order.CustomerId, order.Lines));
            return order;
        }
    }

    public OrderView? Find(Guid id)
    {
        lock (gate) return orders.GetValueOrDefault(id);
    }

    public OrderPlaced? DequeueOutbox()
    {
        lock (gate) return outbox.TryDequeue(out var message) ? message : null;
    }
}

public sealed class InventoryConsumer
{
    private readonly object gate = new();
    private readonly HashSet<Guid> processedMessages = [];
    private readonly Dictionary<string, int> reservations = new(StringComparer.OrdinalIgnoreCase);

    public bool Handle(OrderPlaced message)
    {
        lock (gate)
        {
            if (!processedMessages.Add(message.MessageId)) return false;
            foreach (var line in message.Lines)
                reservations[line.Sku] = reservations.GetValueOrDefault(line.Sku) + line.Quantity;
            return true;
        }
    }

    public int Reserved(string sku)
    {
        lock (gate) return reservations.GetValueOrDefault(sku);
    }
}
