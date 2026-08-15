using MicroservicesOrderFlow;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<OrderService>();
builder.Services.AddSingleton<InventoryConsumer>();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.MapHealthChecks("/health");
app.MapPost("/orders", (PlaceOrder command, OrderService service) =>
{
    try
    {
        var order = service.Place(command);
        return Results.Created($"/orders/{order.Id}", order);
    }
    catch (ArgumentException error)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["order"] = [error.Message] });
    }
});
app.MapGet("/orders/{id:guid}", (Guid id, OrderService service) => service.Find(id) is { } order ? Results.Ok(order) : Results.NotFound());
app.MapPost("/operations/publish-next", (OrderService orders, InventoryConsumer inventory) =>
{
    var message = orders.DequeueOutbox();
    return message is null ? Results.NoContent() : Results.Ok(new { message.MessageId, Applied = inventory.Handle(message) });
});
app.MapGet("/inventory/{sku}/reserved", (string sku, InventoryConsumer inventory) => Results.Ok(new { sku, quantity = inventory.Reserved(sku) }));
app.Run();

public partial class Program;
