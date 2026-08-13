namespace MinimalApisOrders.Orders;

internal sealed record Order(
    Guid Id,
    string CustomerId,
    IReadOnlyList<OrderLine> Lines,
    DateTimeOffset CreatedAtUtc)
{
    public string Status => "Pending";
}

internal sealed record OrderLine(string Sku, int Quantity);
