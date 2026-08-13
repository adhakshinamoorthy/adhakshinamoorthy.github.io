using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MinimalApisOrders.Tests;

public sealed class OrderApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_seeded_order_returns_typed_contract()
    {
        var response = await _client.GetAsync("/api/orders/11111111-1111-1111-1111-111111111111");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("CUS-100", body.GetProperty("customerId").GetString());
        Assert.Equal("Pending", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Create_without_credentials_returns_unauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", ValidRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_with_invalid_body_returns_validation_problem()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders");
        request.Headers.Add("X-Api-Key", "local-development-key");
        request.Content = JsonContent.Create(new { customerId = "", lines = Array.Empty<object>() });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").TryGetProperty("CustomerId", out _));
    }

    [Fact]
    public async Task Create_with_credentials_returns_created_resource_location()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders");
        request.Headers.Add("X-Api-Key", "local-development-key");
        request.Content = JsonContent.Create(ValidRequest());

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var followUp = await _client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, followUp.StatusCode);
    }

    [Fact]
    public async Task OpenApi_document_describes_order_endpoints()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        response.EnsureSuccessStatusCode();
        var document = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/orders", document, StringComparison.Ordinal);
        Assert.Contains("CreateOrder", document, StringComparison.Ordinal);
    }

    private static object ValidRequest() => new
    {
        customerId = "CUS-200",
        lines = new[] { new { sku = "BOOK-2", quantity = 2 } }
    };
}
