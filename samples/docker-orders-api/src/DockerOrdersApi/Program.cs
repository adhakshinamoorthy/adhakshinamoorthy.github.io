using DockerOrdersApi;

if (args.Contains("--healthcheck", StringComparer.Ordinal))
{
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
    try
    {
        using var response = await client.GetAsync("http://127.0.0.1:8080/health/ready");
        return response.IsSuccessStatusCode ? 0 : 1;
    }
    catch (HttpRequestException)
    {
        return 1;
    }
    catch (TaskCanceledException)
    {
        return 1;
    }
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<OrderStore>();

var app = builder.Build();
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" }));
app.MapGet("/container", (IHostEnvironment environment) =>
    Results.Ok(ContainerIdentity.FromEnvironment(environment.EnvironmentName)));
app.MapGet("/orders", (OrderStore store) => Results.Ok(store.List()));
app.MapPost("/orders", (CreateOrder request, OrderStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.Customer) || request.Total <= 0)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["order"] = ["Customer is required and total must be greater than zero."]
        });
    }

    var order = store.Add(request.Customer.Trim(), request.Total);
    return Results.Created($"/orders/{order.Id}", order);
});
app.Run();
return 0;

public partial class Program;
