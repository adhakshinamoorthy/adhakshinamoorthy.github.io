using TestingOrdersApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<OrderService>();

var app = builder.Build();

app.MapPost("/orders", (CreateOrderRequest request, OrderService service) =>
{
    try
    {
        var order = service.Create(request);
        return Results.Created($"/orders/{order.Id}", order);
    }
    catch (ArgumentException exception)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["order"] = [exception.Message]
        });
    }
});

app.MapGet("/orders/{id:guid}", (Guid id, OrderService service) =>
    service.Find(id) is { } order ? Results.Ok(order) : Results.NotFound());

app.Run();

public partial class Program;
