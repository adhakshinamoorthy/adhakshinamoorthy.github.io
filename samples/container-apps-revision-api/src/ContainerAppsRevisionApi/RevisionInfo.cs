namespace ContainerAppsRevisionApi;

public sealed record RevisionInfo(string Name, string Replica, string Region)
{
    public static RevisionInfo FromEnvironment() => new(
        Environment.GetEnvironmentVariable("CONTAINER_APP_REVISION") ?? "local",
        Environment.GetEnvironmentVariable("HOSTNAME") ?? Environment.MachineName,
        Environment.GetEnvironmentVariable("AZURE_REGION") ?? "local");
}

public sealed class ReadinessState
{
    private int ready;
    public bool IsReady => Volatile.Read(ref ready) == 1;
    public void MarkReady() => Interlocked.Exchange(ref ready, 1);
    public void MarkUnready() => Interlocked.Exchange(ref ready, 0);
}

public sealed class LifecycleService(ReadinessState readiness, ILogger<LifecycleService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        readiness.MarkReady();
        logger.LogInformation("Replica is ready to receive traffic");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        readiness.MarkUnready();
        logger.LogInformation("Replica stopped accepting new work");
        return Task.CompletedTask;
    }
}
