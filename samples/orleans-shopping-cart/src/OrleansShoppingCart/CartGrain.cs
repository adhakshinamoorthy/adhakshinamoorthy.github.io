namespace OrleansShoppingCart;

public interface ICartGrain : IGrainWithStringKey
{
    Task<CartSnapshot> GetAsync();
    Task<CartSnapshot> AddAsync(string sku, int quantity);
    Task ClearAsync();
}

[GenerateSerializer]
public sealed record CartLine([property: Id(0)] string Sku, [property: Id(1)] int Quantity);

[GenerateSerializer]
public sealed record CartSnapshot([property: Id(0)] string CartId, [property: Id(1)] IReadOnlyList<CartLine> Lines);

public static class CartRules
{
    public static int Merge(int current, int added)
    {
        if (added is < 1 or > 25) throw new ArgumentOutOfRangeException(nameof(added), "Quantity must be from 1 through 25.");
        return checked(current + added);
    }
}

public sealed class CartGrain : Grain, ICartGrain
{
    private readonly Dictionary<string, int> lines = new(StringComparer.OrdinalIgnoreCase);

    public Task<CartSnapshot> GetAsync() => Task.FromResult(Snapshot());

    public Task<CartSnapshot> AddAsync(string sku, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        lines[sku] = CartRules.Merge(lines.GetValueOrDefault(sku), quantity);
        return Task.FromResult(Snapshot());
    }

    public Task ClearAsync()
    {
        lines.Clear();
        return Task.CompletedTask;
    }

    private CartSnapshot Snapshot() => new(this.GetPrimaryKeyString(), lines.OrderBy(x => x.Key).Select(x => new CartLine(x.Key, x.Value)).ToArray());
}
