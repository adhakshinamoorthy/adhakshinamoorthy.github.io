using HotChocolate.Authorization;

namespace GraphQlInventoryCatalog;

public sealed record Product(Guid Id, string Sku, string Name, int Available, long Version);
public sealed record InventoryError(string Code, string Message);
public sealed record AdjustStockPayload(Product? Product, InventoryError? Error);

public sealed class ProductStore
{
    private readonly object _gate = new();
    private readonly List<Product> _products =
    [
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"), "SKU-BLUE", "Blue mug", 8, 1),
        new(Guid.Parse("00000000-0000-0000-0000-000000000002"), "SKU-GREEN", "Green notebook", 15, 1),
        new(Guid.Parse("00000000-0000-0000-0000-000000000003"), "SKU-RED", "Red pen", 30, 1)
    ];

    public IReadOnlyList<Product> List() { lock (_gate) return _products.OrderBy(item => item.Sku).ToArray(); }
    public Product? Find(Guid id) { lock (_gate) return _products.FirstOrDefault(item => item.Id == id); }
    public Product? Adjust(Guid id, int delta)
    {
        lock (_gate)
        {
            var index = _products.FindIndex(item => item.Id == id);
            if (index < 0 || _products[index].Available + delta < 0) return null;
            return _products[index] = _products[index] with { Available = _products[index].Available + delta, Version = _products[index].Version + 1 };
        }
    }
}

public sealed class Query
{
    [UsePaging(MaxPageSize = 10, IncludeTotalCount = true)]
    public IEnumerable<Product> GetProducts(ProductStore store) => store.List();
    public Product? GetProduct(Guid id, ProductStore store) => store.Find(id);
}

public sealed class Mutation
{
    [Authorize(Policy = "inventory.write")]
    public AdjustStockPayload AdjustStock(Guid id, int delta, ProductStore store)
    {
        if (delta is < -100 or > 100) return new(null, new("DELTA_OUT_OF_RANGE", "Delta must be between -100 and 100."));
        var product = store.Adjust(id, delta);
        return product is null
            ? new(null, new("ADJUSTMENT_REJECTED", "Product was not found or stock would become negative."))
            : new(product, null);
    }
}
