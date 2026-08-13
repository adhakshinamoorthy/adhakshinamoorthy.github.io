using GrpcInventory;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(builder.Configuration["GrpcApiKey"]))
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["GrpcApiKey"] = "local-demo-key" });
}
builder.Services.AddSingleton<InventoryStore>();
builder.Services.AddSingleton<ApiKeyInterceptor>();
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ApiKeyInterceptor>();
    options.MaxReceiveMessageSize = 64 * 1024;
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

var app = builder.Build();
app.MapGrpcService<InventoryService>();
app.MapGet("/", () => Results.Ok(new { service = "inventory.v1", transport = "gRPC over HTTP/2" }));
app.Run();

public partial class Program;
