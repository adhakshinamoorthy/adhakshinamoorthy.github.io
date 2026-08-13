using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiDesignOrders.Tests;

public sealed class ApiContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public ApiContractTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Pagination_is_bounded_and_returns_cursor()
    {
        var page = await _client.GetFromJsonAsync<JsonElement>("/api/orders?limit=2");
        Assert.Equal(2, page.GetProperty("items").GetArrayLength());
        Assert.False(string.IsNullOrWhiteSpace(page.GetProperty("nextCursor").GetString()));
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.GetAsync("/api/orders?limit=1000")).StatusCode);
    }

    [Fact]
    public async Task Idempotency_key_replays_the_same_resource()
    {
        var first = await Create("retry-1", "CUS-900");
        var second = await Create("retry-1", "CUS-900");
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(first.Headers.Location, second.Headers.Location);
    }

    [Fact]
    public async Task Reusing_key_with_different_request_conflicts()
    {
        await Create("retry-2", "CUS-901");
        var response = await Create("retry-2", "CUS-902");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Replace_requires_current_etag()
    {
        var get = await _client.GetAsync("/api/orders/00000000-0000-0000-0000-000000000001");
        var etag = get.Headers.ETag?.Tag;
        Assert.NotNull(etag);

        var without = await _client.PutAsJsonAsync(
            "/api/orders/00000000-0000-0000-0000-000000000001",
            new { customerId = "CUS-001", lines = new[] { new { sku = "SKU-1", quantity = 2 } } });
        Assert.Equal((HttpStatusCode)428, without.StatusCode);

        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/orders/00000000-0000-0000-0000-000000000001");
        request.Headers.TryAddWithoutValidation("If-Match", etag);
        request.Content = JsonContent.Create(new { customerId = "CUS-001", lines = new[] { new { sku = "SKU-1", quantity = 2 } } });
        var replaced = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, replaced.StatusCode);
        Assert.NotEqual(etag, replaced.Headers.ETag?.Tag);
    }

    [Fact]
    public async Task Stale_etag_is_rejected()
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/orders/00000000-0000-0000-0000-000000000002");
        request.Headers.TryAddWithoutValidation("If-Match", "\"0\"");
        request.Content = JsonContent.Create(new { customerId = "CUS-002", lines = new[] { new { sku = "SKU-2", quantity = 2 } } });
        Assert.Equal(HttpStatusCode.PreconditionFailed, (await _client.SendAsync(request)).StatusCode);
    }

    private async Task<HttpResponseMessage> Create(string key, string customerId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders");
        request.Headers.Add("Idempotency-Key", key);
        request.Content = JsonContent.Create(new { customerId, lines = new[] { new { sku = "SKU-X", quantity = 1 } } });
        return await _client.SendAsync(request);
    }
}
