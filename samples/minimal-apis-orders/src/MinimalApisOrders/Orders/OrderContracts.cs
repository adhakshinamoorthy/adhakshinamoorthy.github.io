namespace MinimalApisOrders.Orders;

public sealed record CreateOrderRequest(
    string CustomerId,
    IReadOnlyList<CreateOrderLineRequest> Lines);

public sealed record CreateOrderLineRequest(string Sku, int Quantity);

public sealed record OrderResponse(
    Guid Id,
    string CustomerId,
    string Status,
    IReadOnlyList<OrderLineResponse> Lines,
    DateTimeOffset CreatedAtUtc);

public sealed record OrderLineResponse(string Sku, int Quantity);
