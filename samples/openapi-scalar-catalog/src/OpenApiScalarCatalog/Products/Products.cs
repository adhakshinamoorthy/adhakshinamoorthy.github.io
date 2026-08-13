using System.Collections.Concurrent;

namespace OpenApiScalarCatalog.Products;

public sealed record CreateProductRequest(string Name, decimal Price);
public sealed record ProductResponse(Guid Id, string Name, decimal Price);

internal sealed class ProductCatalog
{
    private readonly ConcurrentDictionary<Guid, ProductResponse> _products = new();

    public ProductCatalog()
    {
        var product = new ProductResponse(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Architecture handbook",
            29.00m);
        _products[product.Id] = product;
    }

    public IReadOnlyList<ProductResponse> List() => _products.Values.OrderBy(product => product.Name).ToArray();
    public ProductResponse? Find(Guid id) => _products.GetValueOrDefault(id);
    public ProductResponse Add(CreateProductRequest request)
    {
        var product = new ProductResponse(Guid.NewGuid(), request.Name.Trim(), request.Price);
        _products[product.Id] = product;
        return product;
    }
}

internal static class ProductValidation
{
    public static Dictionary<string, string[]> Validate(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 120)
            errors["name"] = ["Name is required and must be at most 120 characters."];
        if (request.Price is <= 0 or > 100_000)
            errors["price"] = ["Price must be greater than zero and at most 100000."];
        return errors;
    }
}
