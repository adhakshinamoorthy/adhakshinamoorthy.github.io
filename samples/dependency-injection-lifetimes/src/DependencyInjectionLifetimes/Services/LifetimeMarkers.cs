namespace DependencyInjectionLifetimes.Services;

public sealed class ApplicationIdentity
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed class OperationScope : IDisposable
{
    public Guid Id { get; } = Guid.NewGuid();

    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

public sealed class ActivityIdentity
{
    public Guid Id { get; } = Guid.NewGuid();
}
