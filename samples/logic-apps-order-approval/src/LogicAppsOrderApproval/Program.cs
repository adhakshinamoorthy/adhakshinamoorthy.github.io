using LogicAppsOrderApproval;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ApprovalRegistry>();
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ready" }));
app.MapPost("/callbacks/approvals", (ApprovalCallback callback, ApprovalRegistry registry) =>
{
    try
    {
        var outcome = registry.Record(callback, DateTimeOffset.UtcNow);
        return outcome.Created
            ? Results.Accepted($"/callbacks/approvals/{outcome.Result.WorkflowRunId}", outcome.Result)
            : Results.Ok(outcome.Result);
    }
    catch (ArgumentException exception)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["callback"] = [exception.Message] });
    }
});
app.MapGet("/callbacks/approvals/{runId}", (string runId, ApprovalRegistry registry) =>
    registry.Find(runId) is { } result ? Results.Ok(result) : Results.NotFound());

app.Run();
