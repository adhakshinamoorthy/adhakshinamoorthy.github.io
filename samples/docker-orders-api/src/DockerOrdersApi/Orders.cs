using System.Collections.Concurrent;

namespace DockerOrdersApi;

public sealed record CreateOrder(string Customer, decimal Total);
public sealed record Order(Guid Id, string Customer, decimal Total);
public sealed record ContainerIdentity(string Hostname, string Environment, bool RunningAsRoot)
{
    public static ContainerIdentity FromEnvironment(string environment) => new(
        System.Environment.GetEnvironmentVariable("HOSTNAME") ?? System.Environment.MachineName,
        environment,
        string.Equals(System.Environment.UserName, "root", StringComparison.OrdinalIgnoreCase));
}

public sealed class OrderStore
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Order Add(string customer, decimal total)
    {
        var order = new Order(Guid.NewGuid(), customer, total);
        _orders[order.Id] = order;
        return order;
    }

    public IReadOnlyCollection<Order> List() => _orders.Values.OrderBy(order => order.Customer).ToArray();
}
