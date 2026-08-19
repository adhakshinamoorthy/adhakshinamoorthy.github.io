using KubernetesInventoryApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ReadinessState>();
builder.Services.AddHostedService<StartupCoordinator>();

var app = builder.Build();
app.MapGet("/livez", () => Results.Ok(new { status = "live" }));
app.MapGet("/readyz", (ReadinessState readiness) => readiness.IsReady
    ? Results.Ok(new { status = "ready" })
    : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
app.MapGet("/inventory/{sku}", (string sku, ReadinessState readiness) => readiness.IsReady
    ? Results.Ok(new InventoryItem(sku.ToUpperInvariant(), 25))
    : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
app.Run();

public partial class Program;
