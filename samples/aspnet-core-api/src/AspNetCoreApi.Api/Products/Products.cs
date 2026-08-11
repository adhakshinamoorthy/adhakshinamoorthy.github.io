using System.Collections.Concurrent;

namespace AspNetCoreApi.Products;

public sealed record Product(Guid Id, string Name, decimal Price, DateTimeOffset CreatedAt);

public sealed record CreateProductRequest(string? Name, decimal Price);

public interface IProductRepository
{
    ValueTask<IReadOnlyCollection<Product>> ListAsync(CancellationToken cancellationToken);
    ValueTask<Product?> FindAsync(Guid id, CancellationToken cancellationToken);
    ValueTask<Product> AddAsync(string name, decimal price, CancellationToken cancellationToken);
}

public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    public InMemoryProductRepository()
    {
        var starter = new Product(
            Guid.Parse("d85b1407-351d-4694-9392-03acc5870eb1"),
            "Mechanical keyboard",
            89.00m,
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"));
        _products[starter.Id] = starter;
    }

    public ValueTask<IReadOnlyCollection<Product>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<Product> products = _products.Values
            .OrderBy(product => product.Name)
            .ToArray();
        return ValueTask.FromResult(products);
    }

    public ValueTask<Product?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _products.TryGetValue(id, out var product);
        return ValueTask.FromResult(product);
    }

    public ValueTask<Product> AddAsync(string name, decimal price, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var product = new Product(Guid.NewGuid(), name, price, DateTimeOffset.UtcNow);
        _products[product.Id] = product;
        return ValueTask.FromResult(product);
    }
}
