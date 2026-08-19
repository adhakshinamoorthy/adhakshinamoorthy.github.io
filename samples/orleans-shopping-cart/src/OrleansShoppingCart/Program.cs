using OrleansShoppingCart;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseOrleans(silo => silo.UseLocalhostClustering());
builder.Services.AddHealthChecks();

var app = builder.Build();
app.MapHealthChecks("/health");
app.MapGet("/carts/{cartId}", async (string cartId, IGrainFactory grains) => Results.Ok(await grains.GetGrain<ICartGrain>(cartId).GetAsync()));
app.MapPost("/carts/{cartId}/items/{sku}", async (string cartId, string sku, int quantity, IGrainFactory grains) =>
{
    try { return Results.Ok(await grains.GetGrain<ICartGrain>(cartId).AddAsync(sku, quantity)); }
    catch (ArgumentException error) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["quantity"] = [error.Message] }); }
});
app.MapDelete("/carts/{cartId}", async (string cartId, IGrainFactory grains) => { await grains.GetGrain<ICartGrain>(cartId).ClearAsync(); return Results.NoContent(); });
app.Run();

public partial class Program;
