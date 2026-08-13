namespace ModularMonolithStorefront;

public sealed record ProductSnapshot(string Sku, string Name, decimal Price, int Available);
public sealed record OrderAccepted(Guid OrderId, string Sku, int Quantity, decimal Total);

public interface ICatalogQueries { ProductSnapshot? Find(string sku); }
public interface IInventoryCommands { bool TryReserve(string sku, int quantity); }
public interface IOrderEvents { void Publish(OrderAccepted message); }

public sealed class CatalogModule : ICatalogQueries, IInventoryCommands
{
    private readonly Dictionary<string, ProductSnapshot> products = new(StringComparer.OrdinalIgnoreCase);
    public void Seed(ProductSnapshot product) => products.Add(product.Sku, product);
    public ProductSnapshot? Find(string sku) => products.GetValueOrDefault(sku);
    public bool TryReserve(string sku, int quantity)
    {
        if (quantity <= 0 || !products.TryGetValue(sku, out var product) || product.Available < quantity) return false;
        products[sku] = product with { Available = product.Available - quantity };
        return true;
    }
}

public sealed class OrdersModule(ICatalogQueries catalog, IInventoryCommands inventory, IOrderEvents events)
{
    public OrderAccepted Place(string sku, int quantity)
    {
        if (quantity is < 1 or > 20) throw new ArgumentOutOfRangeException(nameof(quantity));
        var product = catalog.Find(sku) ?? throw new KeyNotFoundException("Product was not found.");
        if (!inventory.TryReserve(product.Sku, quantity)) throw new InvalidOperationException("Insufficient inventory.");
        var accepted = new OrderAccepted(Guid.NewGuid(), product.Sku, quantity, product.Price * quantity);
        events.Publish(accepted);
        return accepted;
    }
}

public sealed class InProcessOrderEvents : IOrderEvents
{
    private readonly List<OrderAccepted> messages = [];
    public IReadOnlyList<OrderAccepted> Messages => messages;
    public void Publish(OrderAccepted message) => messages.Add(message);
}
