namespace ApiDesignOrders.Orders;

public sealed record CreateOrderRequest(string CustomerId, IReadOnlyList<OrderLineRequest> Lines);
public sealed record ReplaceOrderRequest(string CustomerId, IReadOnlyList<OrderLineRequest> Lines);
public sealed record OrderLineRequest(string Sku, int Quantity);
public sealed record OrderLineResponse(string Sku, int Quantity);
public sealed record OrderResponse(Guid Id, string CustomerId, IReadOnlyList<OrderLineResponse> Lines, long Version)
{
    internal static OrderResponse From(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Lines.Select(line => new OrderLineResponse(line.Sku, line.Quantity)).ToArray(),
        order.Version);
}
public sealed record OrderPage(IReadOnlyList<OrderResponse> Items, string? NextCursor);

internal static class OrderValidation
{
    public static Dictionary<string, string[]> Validate(CreateOrderRequest request) =>
        Validate(request.CustomerId, request.Lines);

    public static Dictionary<string, string[]> Validate(ReplaceOrderRequest request) =>
        Validate(request.CustomerId, request.Lines);

    private static Dictionary<string, string[]> Validate(string customerId, IReadOnlyList<OrderLineRequest>? lines)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(customerId)) errors["customerId"] = ["customerId is required."];
        if (lines is null || lines.Count is < 1 or > 25) errors["lines"] = ["Provide between 1 and 25 lines."];
        else if (lines.Any(line => string.IsNullOrWhiteSpace(line.Sku) || line.Quantity is < 1 or > 100))
            errors["lines"] = ["Every line needs a SKU and quantity from 1 to 100."];
        return errors;
    }
}
