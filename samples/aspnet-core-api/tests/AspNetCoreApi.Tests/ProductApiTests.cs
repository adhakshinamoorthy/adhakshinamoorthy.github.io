using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AspNetCoreApi.Products;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AspNetCoreApi.Tests;

public sealed class ProductApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task List_returns_the_seeded_product()
    {
        var products = await _client.GetFromJsonAsync<Product[]>("/api/products");

        Assert.Contains(products!, product =>
            product.Id == Guid.Parse("d85b1407-351d-4694-9392-03acc5870eb1")
            && product.Name == "Mechanical keyboard");
    }

    [Fact]
    public async Task Create_returns_created_and_the_product_can_be_read()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductRequest("USB-C dock", 129.50m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<Product>();
        var loaded = await _client.GetFromJsonAsync<Product>(response.Headers.Location);

        Assert.Equal(created, loaded);
    }

    [Fact]
    public async Task Create_with_invalid_input_returns_validation_problem_details()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductRequest("", 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = document.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("name", out _));
        Assert.True(errors.TryGetProperty("price", out _));
    }

    [Fact]
    public async Task Response_propagates_a_valid_correlation_id()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/products");
        request.Headers.Add("X-Correlation-ID", "atlas-test-request");

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.Equal("atlas-test-request", response.Headers.GetValues("X-Correlation-ID").Single());
    }
}
