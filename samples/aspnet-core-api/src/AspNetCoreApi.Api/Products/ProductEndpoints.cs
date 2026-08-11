namespace AspNetCoreApi.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var products = endpoints.MapGroup("/api/products")
            .WithTags("Products")
            .RequireRateLimiting("api");

        products.MapGet("/", ListAsync);
        products.MapGet("/{id:guid}", FindAsync);
        products.MapPost("/", CreateAsync);

        return endpoints;
    }

    private static async Task<IResult> ListAsync(
        IProductRepository repository,
        CancellationToken cancellationToken)
    {
        var products = await repository.ListAsync(cancellationToken);
        return Results.Ok(products);
    }

    private static async Task<IResult> FindAsync(
        Guid id,
        IProductRepository repository,
        CancellationToken cancellationToken)
    {
        var product = await repository.FindAsync(id, cancellationToken);
        return product is null
            ? Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Product not found",
                detail: $"No product exists with id '{id}'.")
            : Results.Ok(product);
    }

    private static async Task<IResult> CreateAsync(
        CreateProductRequest request,
        IProductRepository repository,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var product = await repository.AddAsync(request.Name!.Trim(), request.Price, cancellationToken);
        return Results.Created($"/api/products/{product.Id}", product);
    }

    private static Dictionary<string, string[]> Validate(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["name"] = ["Name is required."];
        }
        else if (request.Name.Trim().Length > 120)
        {
            errors["name"] = ["Name must be 120 characters or fewer."];
        }

        if (request.Price <= 0)
        {
            errors["price"] = ["Price must be greater than zero."];
        }

        return errors;
    }
}
