using FeatureFlagsCheckout;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(new CheckoutFlag("CheckoutV2", true, 20, ["staff"], "checkout-team", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30))));
builder.Services.AddSingleton<StableRolloutEvaluator>();
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ready" }));
app.MapGet("/checkout", (HttpContext context, StableRolloutEvaluator evaluator) =>
{
    var user = context.Request.Headers["X-User-Id"].FirstOrDefault() ?? "anonymous";
    var groups = (context.Request.Headers["X-Groups"].FirstOrDefault() ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var decision = evaluator.Evaluate(user, groups);
    return Results.Ok(new { experience = decision.Enabled ? "v2" : "current", decision });
});

app.Run();
