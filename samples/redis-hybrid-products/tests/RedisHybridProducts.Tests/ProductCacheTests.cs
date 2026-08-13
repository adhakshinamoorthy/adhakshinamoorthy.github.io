using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RedisHybridProducts.Tests;

public sealed class ProductCacheTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private const string ProductPath = "/api/products/00000000-0000-0000-0000-000000000001";

    public Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() { _client.Dispose(); await _factory.DisposeAsync(); }

    [Fact]
    public async Task Repeated_read_uses_cache()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync(ProductPath)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync(ProductPath)).StatusCode);
        Assert.Equal(1, await Reads());
    }

    [Fact]
    public async Task Concurrent_miss_is_coalesced()
    {
        var requests = Enumerable.Range(0, 20).Select(_ => _client.GetAsync(ProductPath));
        var responses = await Task.WhenAll(requests);
        Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
        Assert.Equal(1, await Reads());
    }

    [Fact]
    public async Task Update_invalidates_cached_value()
    {
        await _client.GetAsync(ProductPath);
        var updated = await _client.PutAsJsonAsync(ProductPath, new { name = "Quiet keyboard", price = 159m });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var product = await _client.GetFromJsonAsync<JsonElement>(ProductPath);
        Assert.Equal("Quiet keyboard", product.GetProperty("name").GetString());
        Assert.Equal(2, await Reads());
    }

    [Fact]
    public async Task Missing_product_returns_not_found()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/products/ffffffff-ffff-ffff-ffff-ffffffffffff")).StatusCode);
    }

    private async Task<int> Reads() => (await _client.GetFromJsonAsync<JsonElement>("/diagnostics/source-reads")).GetProperty("reads").GetInt32();
}
