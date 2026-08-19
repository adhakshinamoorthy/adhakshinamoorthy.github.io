using DockerOrdersApi;
using Xunit;

namespace DockerOrdersApi.Tests;

public sealed class OrderStoreTests
{
    [Fact]
    public void Add_persists_an_order()
    {
        var store = new OrderStore();
        var created = store.Add("Ada", 42.50m);

        var order = Assert.Single(store.List());
        Assert.Equal(created, order);
        Assert.Equal("Ada", order.Customer);
    }

    [Fact]
    public void List_orders_by_customer()
    {
        var store = new OrderStore();
        store.Add("Zoe", 10m);
        store.Add("Ada", 20m);

        Assert.Equal(["Ada", "Zoe"], store.List().Select(order => order.Customer));
    }
}
