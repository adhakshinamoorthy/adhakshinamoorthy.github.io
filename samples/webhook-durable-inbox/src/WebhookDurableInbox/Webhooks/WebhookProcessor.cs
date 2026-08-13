using System.Text.Json;
using System.Threading.Channels;

namespace WebhookDurableInbox.Webhooks;

internal sealed class WebhookWorkQueue
{
    private readonly Channel<string> _channel = Channel.CreateBounded<string>(new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = false
    });

    public ValueTask EnqueueAsync(string deliveryId, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(deliveryId, cancellationToken);

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}

internal sealed class WebhookProcessor(
    FileWebhookInbox inbox,
    WebhookWorkQueue queue,
    ILogger<WebhookProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var pendingId in await inbox.PendingIdsAsync(stoppingToken))
            await queue.EnqueueAsync(pendingId, stoppingToken);

        await foreach (var deliveryId in queue.ReadAllAsync(stoppingToken))
        {
            var delivery = await inbox.BeginAsync(deliveryId, stoppingToken);
            if (delivery is null) continue;
            try
            {
                var payload = Convert.FromBase64String(delivery.PayloadBase64);
                using var document = JsonDocument.Parse(payload);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    throw new InvalidDataException("Webhook payload must be a JSON object.");

                logger.LogInformation(
                    "Processed webhook {DeliveryId} of type {EventType} on attempt {Attempt}",
                    delivery.DeliveryId,
                    delivery.EventType,
                    delivery.Attempts);
                await inbox.CompleteAsync(deliveryId, stoppingToken);
            }
            catch (Exception exception) when (exception is JsonException or FormatException or InvalidDataException)
            {
                logger.LogWarning(exception, "Webhook {DeliveryId} failed validation", deliveryId);
                await inbox.FailAsync(deliveryId, "Payload validation failed.", stoppingToken);
            }
        }
    }
}
