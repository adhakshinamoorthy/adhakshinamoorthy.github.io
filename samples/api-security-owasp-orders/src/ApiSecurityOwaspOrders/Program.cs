using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.RateLimiting;
using ApiSecurityOwaspOrders.Orders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 1_048_576);
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 1_048_576);
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton(new InventoryDestinationPolicy(["inventory.internal"]));
builder.Services.AddAuthentication(LocalIdentityHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, LocalIdentityHandler>(LocalIdentityHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 100;
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

var orders = app.MapGroup("/api/orders").RequireAuthorization().RequireRateLimiting("api");
orders.MapGet("/", (ClaimsPrincipal user, IOrderRepository repository, int? limit) =>
{
    if (limit is < 1 or > 100) return Results.BadRequest(new { error = "limit must be between 1 and 100" });
    var tenant = user.FindFirstValue("tenant")!;
    return Results.Ok(repository.List(tenant, limit ?? 20).Select(OrderResponse.From));
});
orders.MapGet("/{id:guid}", (Guid id, ClaimsPrincipal user, IOrderRepository repository) =>
{
    var order = repository.Find(id);
    var subject = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var tenant = user.FindFirstValue("tenant");
    return order is not null && order.Subject == subject && order.Tenant == tenant
        ? Results.Ok(OrderResponse.From(order))
        : Results.NotFound();
});
orders.MapPost("/", (CreateOrderRequest request, ClaimsPrincipal user, IOrderRepository repository) =>
{
    if (request.Lines.Count is < 1 or > 25 || request.Lines.Any(line => line.Quantity is < 1 or > 100))
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["lines"] = ["Provide 1-25 lines with quantities from 1-100."] });
    var order = repository.Add(user.FindFirstValue("tenant")!, user.FindFirstValue(ClaimTypes.NameIdentifier)!, request);
    return Results.Created($"/api/orders/{order.Id}", OrderResponse.From(order));
});
app.MapGet("/api/inventory-probe", (string destination, InventoryDestinationPolicy policy) =>
    policy.IsAllowed(destination) ? Results.NoContent() : Results.BadRequest(new { error = "Destination is not allowlisted." }))
    .RequireAuthorization();
app.Run();

public partial class Program;

internal sealed class LocalIdentityHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "LocalIdentity";
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var subject = Request.Headers["X-Subject"].ToString();
        var tenant = Request.Headers["X-Tenant"].ToString();
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(tenant)) return Task.FromResult(AuthenticateResult.NoResult());
        Claim[] claims = [new(ClaimTypes.NameIdentifier, subject), new("tenant", tenant)];
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
