namespace KubernetesInventoryApi;

public sealed record InventoryItem(string Sku, int Available);

public sealed class ReadinessState
{
    private int _ready;
    public bool IsReady => Volatile.Read(ref _ready) == 1;
    public void MarkReady() => Interlocked.Exchange(ref _ready, 1);
    public void MarkNotReady() => Interlocked.Exchange(ref _ready, 0);
}

public sealed class StartupCoordinator(ReadinessState readiness, IHostApplicationLifetime lifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(250), stoppingToken);
        readiness.MarkReady();
        lifetime.ApplicationStopping.Register(readiness.MarkNotReady);
    }
}
