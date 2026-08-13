using WebhookDurableInbox.Webhooks;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 256 * 1024);
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(sp =>
{
    var secret = sp.GetRequiredService<IConfiguration>()["WebhookSecret"];
    if (string.IsNullOrWhiteSpace(secret))
    {
        throw new InvalidOperationException("WebhookSecret is required. Set it through user-secrets, an environment variable, or a managed secret provider.");
    }

    return new WebhookSignatureVerifier(secret);
});
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var environment = sp.GetRequiredService<IWebHostEnvironment>();
    var inboxPath = configuration["WebhookInboxPath"]
        ?? Path.Combine(environment.ContentRootPath, "webhook-inbox.json");
    return new FileWebhookInbox(inboxPath);
});
builder.Services.AddSingleton<WebhookWorkQueue>();
builder.Services.AddHostedService<WebhookProcessor>();

var app = builder.Build();
_ = app.Services.GetRequiredService<WebhookSignatureVerifier>();
app.UseExceptionHandler();

app.MapPost("/webhooks/orders", async (
    HttpRequest request,
    WebhookSignatureVerifier verifier,
    FileWebhookInbox inbox,
    WebhookWorkQueue queue,
    CancellationToken cancellationToken) =>
{
    var deliveryId = request.Headers["X-Delivery-Id"].ToString();
    var eventType = request.Headers["X-Event-Type"].ToString();
    var signature = request.Headers["X-Signature-256"].ToString();

    if (string.IsNullOrWhiteSpace(deliveryId) || deliveryId.Length > 100 ||
        string.IsNullOrWhiteSpace(eventType) || eventType.Length > 100)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid delivery metadata",
            detail: "X-Delivery-Id and X-Event-Type are required and must be at most 100 characters.");
    }

    using var buffer = new MemoryStream();
    await request.Body.CopyToAsync(buffer, cancellationToken);
    var rawBody = buffer.ToArray();
    if (!verifier.IsValid(rawBody, signature)) return Results.Unauthorized();

    var accepted = await inbox.AcceptAsync(deliveryId, eventType, rawBody, cancellationToken);
    if (accepted) await queue.EnqueueAsync(deliveryId, cancellationToken);

    return accepted
        ? Results.Accepted($"/webhooks/deliveries/{Uri.EscapeDataString(deliveryId)}")
        : Results.Ok(new { status = "duplicate" });
}).DisableAntiforgery();

app.MapGet("/webhooks/deliveries/{deliveryId}", async (
    string deliveryId,
    FileWebhookInbox inbox,
    CancellationToken cancellationToken) =>
{
    var delivery = await inbox.FindAsync(deliveryId, cancellationToken);
    return delivery is null
        ? Results.NotFound()
        : Results.Ok(new DeliveryStatusResponse(
            delivery.DeliveryId,
            delivery.EventType,
            delivery.Status.ToString().ToLowerInvariant(),
            delivery.Attempts));
});

app.Run();

public partial class Program;
