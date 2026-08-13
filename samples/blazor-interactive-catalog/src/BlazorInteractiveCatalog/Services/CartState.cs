using BlazorInteractiveCatalog.Models;

namespace BlazorInteractiveCatalog.Services;

public sealed class CartState
{
    private readonly Dictionary<int, CartLine> _lines = [];

    public IReadOnlyCollection<CartLine> Lines => _lines.Values;
    public int TotalItems => _lines.Values.Sum(line => line.Quantity);
    public decimal Total => _lines.Values.Sum(line => line.Product.Price * line.Quantity);

    public void Add(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        _lines[product.Id] = _lines.TryGetValue(product.Id, out var current)
            ? current with { Quantity = current.Quantity + 1 }
            : new CartLine(product, 1);
    }

    public void Clear() => _lines.Clear();
}

public sealed record CartLine(Product Product, int Quantity);
