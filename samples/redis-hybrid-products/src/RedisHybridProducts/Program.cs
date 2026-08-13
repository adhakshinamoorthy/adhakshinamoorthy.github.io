using Microsoft.Extensions.Caching.Hybrid;
using RedisHybridProducts;

var builder = WebApplication.CreateBuilder(args);
var redis = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redis))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redis;
        options.InstanceName = "dotnet-atlas:";
    });
}
else if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDistributedMemoryCache();
}
else
{
    throw new InvalidOperationException("ConnectionStrings:Redis is required outside Development and Testing.");
}

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(2),
        LocalCacheExpiration = TimeSpan.FromSeconds(20)
    };
    options.MaximumPayloadBytes = 64 * 1024;
    options.MaximumKeyLength = 200;
});
builder.Services.AddSingleton<ProductSource>();
builder.Services.AddSingleton<ProductCache>();

var app = builder.Build();
app.MapGet("/api/products/{id:guid}", async (Guid id, ProductCache products, CancellationToken ct) =>
    await products.FindAsync(id, ct) is { } product ? Results.Ok(product) : Results.NotFound());
app.MapPut("/api/products/{id:guid}", async (Guid id, UpdateProductRequest request, ProductCache products, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 100 || request.Price is < 0 or > 100_000)
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["product"] = ["Name and price are outside allowed bounds."] });
    return await products.UpdateAsync(id, request, ct) is { } product ? Results.Ok(product) : Results.NotFound();
});
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
    app.MapGet("/diagnostics/source-reads", (ProductSource source) => Results.Ok(new { source.Reads }));
app.Run();

public partial class Program;
