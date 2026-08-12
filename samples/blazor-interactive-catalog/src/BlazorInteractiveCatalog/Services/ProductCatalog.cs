using System.Collections.Immutable;
using BlazorInteractiveCatalog.Models;

namespace BlazorInteractiveCatalog.Services;

public sealed class ProductCatalog
{
    private static readonly ImmutableArray<Product> Products =
    [
        new(1, "Architecture Field Guide", "Book", "Practical boundaries and deployment decisions.", 34.00m),
        new(2, "Observability Cards", "Reference", "Tracing, metrics, and logging prompts for design reviews.", 18.50m),
        new(3, "API Contract Kit", "Workshop", "Exercises for stable HTTP contracts and problem details.", 42.00m)
    ];

    public IReadOnlyList<Product> GetProducts() => Products;
}
