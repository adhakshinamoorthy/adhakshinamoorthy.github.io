using System.Collections.Concurrent;

namespace GrpcInventory;

internal sealed class InventoryStore
{
    private readonly ConcurrentDictionary<string, (int Available, long Version)> _stock = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SKU-RED"] = (12, 1),
        ["SKU-BLUE"] = (7, 1),
        ["SKU-GREEN"] = (0, 1)
    };

    public bool TryGet(string sku, out (int Available, long Version) stock) => _stock.TryGetValue(sku, out stock);
}
