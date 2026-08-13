using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Hybrid;

namespace RedisHybridProducts;

public sealed record Product(Guid Id, string Name, decimal Price, long Version);
public sealed record UpdateProductRequest(string Name, decimal Price);

internal sealed class ProductSource
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new(new[]
    {
        new KeyValuePair<Guid, Product>(Guid.Parse("00000000-0000-0000-0000-000000000001"), new(Guid.Parse("00000000-0000-0000-0000-000000000001"), "Mechanical keyboard", 149m, 1)),
        new KeyValuePair<Guid, Product>(Guid.Parse("00000000-0000-0000-0000-000000000002"), new(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Ergonomic mouse", 89m, 1))
    });
    private int _reads;
    public int Reads => Volatile.Read(ref _reads);

    public async Task<Product?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _reads);
        await Task.Delay(30, cancellationToken);
        return _products.GetValueOrDefault(id);
    }

    public Product? Update(Guid id, UpdateProductRequest request)
    {
        while (_products.TryGetValue(id, out var current))
        {
            var next = current with { Name = request.Name.Trim(), Price = request.Price, Version = current.Version + 1 };
            if (_products.TryUpdate(id, next, current)) return next;
        }
        return null;
    }
}

internal sealed class ProductCache(HybridCache cache, ProductSource source)
{
    private static string Key(Guid id) => $"products:v1:{id:N}";

    public ValueTask<Product?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync<Product?>(Key(id), token => new ValueTask<Product?>(source.FindAsync(id, token)), cancellationToken: cancellationToken);

    public async Task<Product?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var updated = source.Update(id, request);
        if (updated is not null) await cache.RemoveAsync(Key(id), cancellationToken);
        return updated;
    }
}
