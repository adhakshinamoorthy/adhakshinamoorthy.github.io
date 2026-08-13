using ApiDesignOrders.Orders;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);
builder.Services.AddSingleton<OrderStore>();

var app = builder.Build();
app.UseExceptionHandler();

var orders = app.MapGroup("/api/orders").WithTags("Orders");

orders.MapGet("/", (int? limit, string? cursor, OrderStore store) =>
{
    if (limit is < 1 or > 50)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid page size",
            detail: "limit must be between 1 and 50.");
    }

    if (!OrderCursor.TryParse(cursor, out var parsedCursor))
    {
        return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid cursor",
            detail: "cursor is not a valid opaque order cursor.");
    }

    return Results.Ok(store.List(parsedCursor, limit ?? 20));
}).WithName("ListOrders");

orders.MapGet("/{id:guid}", (Guid id, OrderStore store, HttpResponse response) =>
{
    var order = store.Find(id);
    if (order is null) return Results.NotFound();
    response.Headers.ETag = order.ETag;
    return Results.Ok(OrderResponse.From(order));
}).WithName("GetOrder");

orders.MapPost("/", (
    [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
    CreateOrderRequest request,
    OrderStore store) =>
{
    if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid idempotency key",
            detail: "Idempotency-Key is required and must be at most 100 characters.");
    }

    var validation = OrderValidation.Validate(request);
    if (validation.Count > 0) return Results.ValidationProblem(validation);

    var result = store.Create(idempotencyKey, request);
    if (result.Status == CreateOrderStatus.Conflict)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Idempotency key conflict",
            detail: "The key was already used with a different request.");
    }

    var response = OrderResponse.From(result.Order!);
    return Results.CreatedAtRoute("GetOrder", new { id = response.Id }, response);
}).WithName("CreateOrder");

orders.MapPut("/{id:guid}", (
    Guid id,
    [FromHeader(Name = "If-Match")] string? ifMatch,
    ReplaceOrderRequest request,
    OrderStore store,
    HttpResponse response) =>
{
    if (string.IsNullOrWhiteSpace(ifMatch))
    {
        return Results.Problem(
            statusCode: StatusCodes.Status428PreconditionRequired,
            title: "Precondition required",
            detail: "Send the current ETag in If-Match.");
    }

    var validation = OrderValidation.Validate(request);
    if (validation.Count > 0) return Results.ValidationProblem(validation);

    var result = store.Replace(id, ifMatch, request);
    if (result.Status == ReplaceOrderStatus.NotFound) return Results.NotFound();
    if (result.Status == ReplaceOrderStatus.PreconditionFailed)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status412PreconditionFailed,
            title: "Precondition failed",
            detail: "The order changed after it was read. Fetch it again and retry intentionally.");
    }

    response.Headers.ETag = result.Order!.ETag;
    return Results.Ok(OrderResponse.From(result.Order));
}).WithName("ReplaceOrder");

app.Run();

public partial class Program;
