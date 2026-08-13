using ModularMonolithStorefront;
using Xunit;

public sealed class ModuleTests
{
    [Fact] public void Order_collaborates_through_catalog_contracts()
    {
        var catalog = new CatalogModule(); catalog.Seed(new("SKU-1", "Keyboard", 50m, 3));
        var events = new InProcessOrderEvents();
        var accepted = new OrdersModule(catalog, catalog, events).Place("SKU-1", 2);
        Assert.Equal(100m, accepted.Total); Assert.Equal(1, catalog.Find("SKU-1")!.Available); Assert.Equal(accepted, Assert.Single(events.Messages));
    }

    [Fact] public void Failed_reservation_does_not_publish_an_event()
    {
        var catalog = new CatalogModule(); catalog.Seed(new("SKU-1", "Keyboard", 50m, 1));
        var events = new InProcessOrderEvents();
        Assert.Throws<InvalidOperationException>(() => new OrdersModule(catalog, catalog, events).Place("SKU-1", 2));
        Assert.Empty(events.Messages); Assert.Equal(1, catalog.Find("SKU-1")!.Available);
    }

    [Fact] public void Orders_module_can_be_tested_against_public_contracts()
    {
        var events = new InProcessOrderEvents();
        var accepted = new OrdersModule(new StubCatalog(), new StubInventory(), events).Place("SKU-X", 1);
        Assert.Equal(12m, accepted.Total);
    }

    private sealed class StubCatalog : ICatalogQueries { public ProductSnapshot? Find(string sku) => new(sku, "Stub", 12m, 1); }
    private sealed class StubInventory : IInventoryCommands { public bool TryReserve(string sku, int quantity) => true; }
}
