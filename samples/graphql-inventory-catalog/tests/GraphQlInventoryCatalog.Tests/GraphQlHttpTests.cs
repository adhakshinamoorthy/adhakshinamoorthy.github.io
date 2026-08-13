using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GraphQlInventoryCatalog.Tests;

public sealed class GraphQlHttpTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public GraphQlHttpTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Query_returns_only_selected_fields_with_bounded_page()
    {
        var result = await Execute("query { products(first: 2) { totalCount nodes { sku available } pageInfo { hasNextPage endCursor } } }");
        var products = result.GetProperty("data").GetProperty("products");
        Assert.Equal(3, products.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, products.GetProperty("nodes").GetArrayLength());
        Assert.True(products.GetProperty("pageInfo").GetProperty("hasNextPage").GetBoolean());
        Assert.False(products.GetProperty("nodes")[0].TryGetProperty("name", out _));
    }

    [Fact]
    public async Task Anonymous_mutation_is_denied()
    {
        var result = await Execute("mutation { adjustStock(id: \"00000000-0000-0000-0000-000000000001\", delta: 1) { product { available } error { code } } }");
        Assert.Equal("AUTH_NOT_AUTHENTICATED", result.GetProperty("errors")[0].GetProperty("extensions").GetProperty("code").GetString());
    }

    [Fact]
    public async Task Authorized_mutation_returns_typed_payload()
    {
        using var request = Request("mutation { adjustStock(id: \"00000000-0000-0000-0000-000000000001\", delta: 2) { product { sku available version } error { code } } }");
        request.Headers.Add("X-User-Id", "inventory-operator");
        request.Headers.Add("X-Permission", "inventory.write");
        var result = await Read(await _client.SendAsync(request));
        var payload = result.GetProperty("data").GetProperty("adjustStock");
        Assert.Equal("SKU-BLUE", payload.GetProperty("product").GetProperty("sku").GetString());
        Assert.Equal(JsonValueKind.Null, payload.GetProperty("error").ValueKind);
    }

    [Fact]
    public async Task Invalid_domain_change_returns_stable_error_payload()
    {
        using var request = Request("mutation { adjustStock(id: \"00000000-0000-0000-0000-000000000001\", delta: 101) { product { sku } error { code message } } }");
        request.Headers.Add("X-User-Id", "inventory-operator");
        request.Headers.Add("X-Permission", "inventory.write");
        var result = await Read(await _client.SendAsync(request));
        Assert.Equal("DELTA_OUT_OF_RANGE", result.GetProperty("data").GetProperty("adjustStock").GetProperty("error").GetProperty("code").GetString());
    }

    private async Task<JsonElement> Execute(string query) => await Read(await _client.SendAsync(Request(query)));
    private static HttpRequestMessage Request(string query) => new(HttpMethod.Post, "/graphql") { Content = JsonContent.Create(new { query }) };
    private static async Task<JsonElement> Read(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
    }
}
