using System.Text.Json;

namespace WebhookDurableInbox.Webhooks;

internal enum DeliveryStatus { Pending, Processing, Completed, Failed }

internal sealed record WebhookDelivery(
    string DeliveryId,
    string EventType,
    string PayloadBase64,
    DateTimeOffset ReceivedAtUtc,
    DeliveryStatus Status,
    int Attempts,
    string? LastError);

internal sealed record DeliveryStatusResponse(string DeliveryId, string EventType, string Status, int Attempts);

internal sealed class FileWebhookInbox
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly string _path;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FileWebhookInbox(string path)
    {
        _path = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    }

    public async Task<bool> AcceptAsync(string deliveryId, string eventType, byte[] rawPayload, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var state = await LoadAsync(cancellationToken);
            if (state.Deliveries.Any(item => item.DeliveryId == deliveryId)) return false;
            state.Deliveries.Add(new WebhookDelivery(
                deliveryId,
                eventType,
                Convert.ToBase64String(rawPayload),
                DateTimeOffset.UtcNow,
                DeliveryStatus.Pending,
                0,
                null));
            await SaveAsync(state, cancellationToken);
            return true;
        }
        finally { _gate.Release(); }
    }

    public async Task<WebhookDelivery?> FindAsync(string deliveryId, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try { return (await LoadAsync(cancellationToken)).Deliveries.FirstOrDefault(item => item.DeliveryId == deliveryId); }
        finally { _gate.Release(); }
    }

    public async Task<IReadOnlyList<string>> PendingIdsAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return (await LoadAsync(cancellationToken)).Deliveries
                .Where(item => item.Status is DeliveryStatus.Pending or DeliveryStatus.Processing)
                .Select(item => item.DeliveryId)
                .ToArray();
        }
        finally { _gate.Release(); }
    }

    public async Task<WebhookDelivery?> BeginAsync(string deliveryId, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var state = await LoadAsync(cancellationToken);
            var index = state.Deliveries.FindIndex(item => item.DeliveryId == deliveryId);
            if (index < 0 || state.Deliveries[index].Status == DeliveryStatus.Completed) return null;
            var delivery = state.Deliveries[index] with { Status = DeliveryStatus.Processing, Attempts = state.Deliveries[index].Attempts + 1 };
            state.Deliveries[index] = delivery;
            await SaveAsync(state, cancellationToken);
            return delivery;
        }
        finally { _gate.Release(); }
    }

    public async Task CompleteAsync(string deliveryId, CancellationToken cancellationToken)
    {
        await UpdateAsync(deliveryId, item => item with { Status = DeliveryStatus.Completed, LastError = null }, cancellationToken);
    }

    public async Task FailAsync(string deliveryId, string error, CancellationToken cancellationToken)
    {
        await UpdateAsync(deliveryId, item => item with { Status = DeliveryStatus.Failed, LastError = error }, cancellationToken);
    }

    private async Task UpdateAsync(string id, Func<WebhookDelivery, WebhookDelivery> update, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var state = await LoadAsync(cancellationToken);
            var index = state.Deliveries.FindIndex(item => item.DeliveryId == id);
            if (index < 0) return;
            state.Deliveries[index] = update(state.Deliveries[index]);
            await SaveAsync(state, cancellationToken);
        }
        finally { _gate.Release(); }
    }

    private async Task<InboxState> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_path)) return new InboxState();
        await using var stream = File.OpenRead(_path);
        return await JsonSerializer.DeserializeAsync<InboxState>(stream, JsonOptions, cancellationToken) ?? new InboxState();
    }

    private async Task SaveAsync(InboxState state, CancellationToken cancellationToken)
    {
        var temporaryPath = _path + ".tmp";
        await using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        File.Move(temporaryPath, _path, true);
    }

    private sealed class InboxState
    {
        public List<WebhookDelivery> Deliveries { get; init; } = [];
    }
}
