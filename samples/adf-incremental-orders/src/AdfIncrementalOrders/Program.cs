using AdfIncrementalOrders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IncrementalOrderPipeline>();
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ready" }));
app.MapPost("/source/orders", (OrderChange change, IncrementalOrderPipeline pipeline) =>
{
    try { return Results.Accepted(value: pipeline.AddSourceChange(change)); }
    catch (ArgumentException exception) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["order"] = [exception.Message] }); }
});
app.MapPost("/pipeline/run", (IncrementalOrderPipeline pipeline) => Results.Ok(pipeline.Run()));
app.MapGet("/pipeline", (IncrementalOrderPipeline pipeline) => Results.Ok(pipeline.Status()));

app.Run();
