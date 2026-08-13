using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using MinimalApisOrders.Orders;
using MinimalApisOrders.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
});
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services
    .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName,
        _ => { });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(OrderPolicies.Write, policy => policy
        .RequireAuthenticatedUser()
        .RequireClaim("scope", OrderPolicies.Write));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("orders", limiter =>
    {
        limiter.PermitLimit = 60;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next(context);
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => TypedResults.Ok(new
{
    name = "Minimal APIs Orders",
    endpoints = new[] { "/api/orders", "/openapi/v1.json" }
})).ExcludeFromDescription();
app.MapOrderEndpoints();

app.Run();

public partial class Program;
