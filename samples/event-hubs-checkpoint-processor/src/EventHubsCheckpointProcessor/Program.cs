using EventHubsCheckpointProcessor;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<LocalEventHub>();
builder.Services.AddSingleton<CheckpointStore>();
builder.Services.AddSingleton<IdempotentTelemetrySink>();
builder.Services.AddSingleton<TelemetryProcessor>();
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ready" }));
app.MapPost("/events", (TelemetryInput input, LocalEventHub hub) =>
{
    try
    {
        return Results.Accepted(value: hub.Publish(input, DateTimeOffset.UtcNow));
    }
    catch (ArgumentException exception)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["event"] = [exception.Message] });
    }
});
app.MapPost("/partitions/{partitionId}/process", async (string partitionId, int? maximum, TelemetryProcessor processor, CancellationToken ct) =>
{
    if (!int.TryParse(partitionId, out var partition) || partition is < 0 or > 3) return Results.NotFound();
    var processed = await processor.ProcessAsync(partitionId, Math.Clamp(maximum ?? 100, 1, 500), ct);
    return Results.Ok(new { partitionId, processed });
});
app.MapGet("/processor", (CheckpointStore checkpoints, IdempotentTelemetrySink sink) =>
    Results.Ok(new { processedEvents = sink.Count, checkpoints = checkpoints.Snapshot() }));

app.Run();
