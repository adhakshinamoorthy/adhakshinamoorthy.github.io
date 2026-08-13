using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MinimalApisOrders.Orders;

internal static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireRateLimiting("orders");

        group.MapGet("/", List)
            .WithName("ListOrders")
            .WithSummary("List the newest orders")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetOrder")
            .WithSummary("Get one order by identifier");

        group.MapPost("/", Create)
            .WithName("CreateOrder")
            .WithSummary("Create an order")
            .AddEndpointFilter<CreateOrderValidationFilter>()
            .ProducesValidationProblem()
            .RequireAuthorization(OrderPolicies.Write);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteOrder")
            .WithSummary("Delete an order")
            .RequireAuthorization(OrderPolicies.Write);

        return endpoints;
    }

    private static Results<Ok<IReadOnlyList<OrderResponse>>, BadRequest<ProblemDetails>> List(
        IOrderRepository repository,
        int? limit)
    {
        if (limit is < 1 or > 100)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Title = "Invalid page size",
                Detail = "The limit must be between 1 and 100.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        return TypedResults.Ok<IReadOnlyList<OrderResponse>>(
            repository.List(limit ?? 20).Select(ToResponse).ToArray());
    }

    private static Results<Ok<OrderResponse>, NotFound> GetById(
        Guid id,
        IOrderRepository repository)
    {
        var order = repository.Find(id);
        return order is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ToResponse(order));
    }

    private static CreatedAtRoute<OrderResponse> Create(
        [FromBody] CreateOrderRequest request,
        IOrderRepository repository)
    {
        var created = repository.Add(request);
        return TypedResults.CreatedAtRoute(
            ToResponse(created),
            "GetOrder",
            new { id = created.Id });
    }

    private static Results<NoContent, NotFound> Delete(
        Guid id,
        IOrderRepository repository) => repository.Delete(id)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();

    private static OrderResponse ToResponse(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Status,
        order.Lines.Select(line => new OrderLineResponse(line.Sku, line.Quantity)).ToArray(),
        order.CreatedAtUtc);
}

internal static class OrderPolicies
{
    public const string Write = "orders.write";
}
