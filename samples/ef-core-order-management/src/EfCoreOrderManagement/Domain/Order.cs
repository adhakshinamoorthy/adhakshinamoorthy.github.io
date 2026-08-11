namespace EfCoreOrderManagement.Domain;

public enum OrderStatus
{
    Placed,
    Paid,
    Cancelled
}

public sealed class Address
{
    private Address()
    {
    }

    public Address(string line1, string city, string countryCode)
    {
        Line1 = Require(line1, nameof(line1));
        City = Require(city, nameof(city));
        CountryCode = Require(countryCode, nameof(countryCode)).ToUpperInvariant();

        if (CountryCode.Length != 2)
        {
            throw new ArgumentException("Use a two-letter country code.", nameof(countryCode));
        }
    }

    public string Line1 { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string CountryCode { get; private set; } = string.Empty;

    private static string Require(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value.Trim();
}

public sealed record OrderLineInput(string ProductCode, string Description, decimal UnitPrice, int Quantity);

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    private Order(Guid id, Guid customerId, Address shippingAddress, DateTime createdAtUtc)
    {
        Id = id;
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        CreatedAtUtc = DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc);
        Status = OrderStatus.Placed;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public Customer Customer { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }

    public OrderStatus Status { get; private set; }

    public int Version { get; private set; }

    public Address ShippingAddress { get; private set; } = null!;

    public IReadOnlyCollection<OrderItem> Items => _items;

    public static Order Place(Guid customerId, Address shippingAddress, IEnumerable<OrderLineInput> lines, DateTime utcNow)
    {
        var order = new Order(Guid.NewGuid(), customerId, shippingAddress, utcNow);

        foreach (var line in lines)
        {
            order._items.Add(OrderItem.Create(order.Id, line));
        }

        if (order._items.Count == 0)
        {
            throw new ArgumentException("An order needs at least one line.", nameof(lines));
        }

        return order;
    }

    public void MarkPaid()
    {
        if (Status != OrderStatus.Placed)
        {
            throw new InvalidOperationException("Only a placed order can be paid.");
        }

        Status = OrderStatus.Paid;
        Version++;
    }
}

public sealed class OrderItem
{
    private OrderItem()
    {
    }

    private OrderItem(Guid id, Guid orderId, string productCode, string description, decimal unitPrice, int quantity)
    {
        if (unitPrice <= 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        Id = id;
        OrderId = orderId;
        ProductCode = Require(productCode, nameof(productCode)).ToUpperInvariant();
        Description = Require(description, nameof(description));
        UnitPrice = decimal.Round(unitPrice, 2, MidpointRounding.ToEven);
        Quantity = quantity;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public string ProductCode { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    internal static OrderItem Create(Guid orderId, OrderLineInput line) =>
        new(Guid.NewGuid(), orderId, line.ProductCode, line.Description, line.UnitPrice, line.Quantity);

    private static string Require(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value.Trim();
}
