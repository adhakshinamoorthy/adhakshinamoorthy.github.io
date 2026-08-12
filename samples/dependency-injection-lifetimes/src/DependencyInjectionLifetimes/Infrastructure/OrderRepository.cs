using DependencyInjectionLifetimes.Domain;

namespace DependencyInjectionLifetimes.Infrastructure;

public interface IOrderRepository
{
    Task SaveAsync(FulfillmentRequest request, CancellationToken cancellationToken);
}

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<FulfillmentRequest> _orders = [];

    public Task SaveAsync(FulfillmentRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _orders.Add(request);
        return Task.CompletedTask;
    }
}
