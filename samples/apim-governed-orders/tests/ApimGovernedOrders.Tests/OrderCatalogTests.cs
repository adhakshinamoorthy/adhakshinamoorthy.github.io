using ApimGovernedOrders;
using Xunit;

namespace ApimGovernedOrders.Tests;

public sealed class OrderCatalogTests
{
    [Fact]
    public void Create_persists_a_valid_order()
    {
        var catalog = new OrderCatalog();
        var created = catalog.Create(new CreateOrder("customer-42", 125.50m));

        Assert.Equal(created, catalog.Find(created.Id));
        Assert.Equal("accepted", created.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_totals(decimal total)
    {
        var catalog = new OrderCatalog();
        Assert.Throws<ArgumentOutOfRangeException>(() => catalog.Create(new CreateOrder("customer-42", total)));
    }
}
