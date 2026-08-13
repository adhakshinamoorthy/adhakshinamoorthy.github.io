using CleanArchitectureOrders.Application;
using CleanArchitectureOrders.Domain;
using Xunit;

public sealed class ArchitectureTests
{
    [Fact] public void Domain_has_no_infrastructure_dependency() => Assert.DoesNotContain(typeof(Order).Assembly.GetReferencedAssemblies(), assembly => assembly.Name!.Contains("EntityFramework", StringComparison.Ordinal));

    [Fact] public async Task Use_case_depends_on_ports_and_is_deterministic()
    {
        var repository = new RecordingRepository();
        var now = new DateTimeOffset(2026, 8, 13, 10, 0, 0, TimeSpan.Zero);
        var receipt = await new PlaceOrderHandler(repository, new FixedClock(now)).HandleAsync(new("customer-1", [new("sku-1", 2, 5m)]));
        Assert.Equal(10m, receipt.Total); Assert.Equal(now, receipt.AcceptedAt); Assert.Equal(receipt.OrderId, repository.Saved!.Id);
    }

    [Fact] public async Task Invalid_domain_input_is_rejected_before_persistence()
    {
        var repository = new RecordingRepository();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => new PlaceOrderHandler(repository, new FixedClock(default)).HandleAsync(new("customer-1", [new("sku", 0, 5m)])));
        Assert.Null(repository.Saved);
    }

    private sealed class RecordingRepository : IOrderRepository
    {
        public Order? Saved { get; private set; }
        public Task AddAsync(Order order, CancellationToken cancellationToken) { Saved = order; return Task.CompletedTask; }
        public Task<Order?> FindAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Saved?.Id == id ? Saved : null);
    }
    private sealed class FixedClock(DateTimeOffset value) : IClock { public DateTimeOffset UtcNow => value; }
}
