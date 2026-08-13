namespace MinimalApisOrders.Orders;

internal sealed class CreateOrderValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<CreateOrderRequest>().Single();
        var errors = Validate(request);

        return errors.Count > 0
            ? TypedResults.ValidationProblem(errors)
            : await next(context);
    }

    private static Dictionary<string, string[]> Validate(CreateOrderRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.CustomerId))
        {
            errors[nameof(request.CustomerId)] = ["CustomerId is required."];
        }

        if (request.Lines is null || request.Lines.Count == 0)
        {
            errors[nameof(request.Lines)] = ["At least one order line is required."];
            return errors;
        }

        if (request.Lines.Any(line => string.IsNullOrWhiteSpace(line.Sku)))
        {
            errors["Lines.Sku"] = ["Every line needs a SKU."];
        }

        if (request.Lines.Any(line => line.Quantity is < 1 or > 100))
        {
            errors["Lines.Quantity"] = ["Quantity must be between 1 and 100."];
        }

        return errors;
    }
}
