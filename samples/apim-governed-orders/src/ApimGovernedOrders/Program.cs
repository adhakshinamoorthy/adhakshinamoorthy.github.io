using ApimGovernedOrders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<OrderCatalog>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    const string header = "x-correlation-id";
    var correlationId = context.Request.Headers[header].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(correlationId)) correlationId = context.TraceIdentifier;
    context.Response.Headers[header] = correlationId;
    using (app.Logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
    {
        await next(context);
    }
});

app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" }));
app.MapGet("/health", () => Results.Ok(new { status = "ready" }));

var orders = app.MapGroup("/orders");
orders.MapPost("/", (CreateOrder request, OrderCatalog catalog) =>
{
    try
    {
        var order = catalog.Create(request);
        return Results.Created($"/orders/{order.Id}", order);
    }
    catch (ArgumentException exception)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["order"] = [exception.Message] });
    }
});
orders.MapGet("/{id:guid}", (Guid id, OrderCatalog catalog) =>
    catalog.Find(id) is { } order ? Results.Ok(order) : Results.NotFound());

app.Run();
