namespace BlazorInteractiveCatalog.Models;

public sealed record Product(
    int Id,
    string Name,
    string Category,
    string Description,
    decimal Price);
