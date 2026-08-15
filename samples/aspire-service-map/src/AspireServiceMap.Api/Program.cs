using AspireServiceMap.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();

var app = builder.Build();
app.MapHealthChecks("/health");
app.MapGet("/topology", () => Results.Ok(new
{
    resources = new[] { "catalog-api", "orders-api" },
    references = new[] { new ResourceReference("orders-api", "catalog-api") }
}));
app.Run();

public partial class Program;
