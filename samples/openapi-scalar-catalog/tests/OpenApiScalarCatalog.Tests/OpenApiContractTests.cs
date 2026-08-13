using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace OpenApiScalarCatalog.Tests;

public sealed class OpenApiContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OpenApiContractTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development")).CreateClient();
    }

    [Fact]
    public async Task Document_is_openapi_31_and_contains_stable_operations()
    {
        var document = await ReadDocument();
        Assert.Equal("3.1.1", document.GetProperty("openapi").GetString());
        var paths = document.GetProperty("paths");
        Assert.Equal("ListProducts", paths.GetProperty("/api/products").GetProperty("get").GetProperty("operationId").GetString());
        Assert.Equal("GetProduct", paths.GetProperty("/api/products/{id}").GetProperty("get").GetProperty("operationId").GetString());
        Assert.Equal("CreateProduct", paths.GetProperty("/api/products").GetProperty("post").GetProperty("operationId").GetString());
    }

    [Fact]
    public async Task Create_operation_documents_success_and_validation_responses()
    {
        var operation = (await ReadDocument()).GetProperty("paths").GetProperty("/api/products").GetProperty("post");
        var responses = operation.GetProperty("responses");
        Assert.True(responses.TryGetProperty("201", out _));
        Assert.True(responses.TryGetProperty("400", out _));
    }

    [Fact]
    public async Task Document_contains_request_and_response_schemas()
    {
        var serialized = (await ReadDocument()).ToString();
        Assert.Contains("CreateProductRequest", serialized, StringComparison.Ordinal);
        Assert.Contains("ProductResponse", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Scalar_reference_is_available_in_development()
    {
        var response = await _client.GetAsync("/scalar/v1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Catalog API reference", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    private async Task<JsonElement> ReadDocument()
    {
        var json = await _client.GetStringAsync("/openapi/v1.json");
        return JsonSerializer.Deserialize<JsonElement>(json);
    }
}
