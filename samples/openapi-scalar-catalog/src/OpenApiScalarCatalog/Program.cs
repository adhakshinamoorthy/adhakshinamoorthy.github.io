using Microsoft.AspNetCore.Http.HttpResults;
using OpenApiScalarCatalog.Products;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ProductCatalog>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Catalog API reference"));
}

var products = app.MapGroup("/api/products")
    .WithTags("Products");

products.MapGet("/", (ProductCatalog catalog) =>
        TypedResults.Ok<IReadOnlyList<ProductResponse>>(catalog.List()))
    .WithName("ListProducts")
    .WithSummary("Lists catalog products")
    .WithDescription("Returns the complete bounded sample catalog ordered by name.");

products.MapGet("/{id:guid}", Results<Ok<ProductResponse>, NotFound> (
    Guid id,
    ProductCatalog catalog) =>
{
    var product = catalog.Find(id);
    return product is null ? TypedResults.NotFound() : TypedResults.Ok(product);
})
    .WithName("GetProduct")
    .WithSummary("Gets one product")
    .WithDescription("Returns 404 when the product identifier is unknown.");

products.MapPost("/", Results<CreatedAtRoute<ProductResponse>, ValidationProblem> (
    CreateProductRequest request,
    ProductCatalog catalog) =>
{
    var errors = ProductValidation.Validate(request);
    if (errors.Count > 0) return TypedResults.ValidationProblem(errors);
    var product = catalog.Add(request);
    return TypedResults.CreatedAtRoute(product, "GetProduct", new { id = product.Id });
})
    .WithName("CreateProduct")
    .WithSummary("Creates a catalog product")
    .WithDescription("Validates the public request model and returns a stable product representation.")
    .ProducesValidationProblem();

app.Run();

public partial class Program;
