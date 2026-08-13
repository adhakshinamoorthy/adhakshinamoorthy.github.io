using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MultiTenantInvoices.Tests;

public sealed class TenantIsolationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public TenantIsolationTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Anonymous_request_is_denied() => Assert.Equal(HttpStatusCode.Unauthorized, (await _factory.CreateClient().GetAsync("/api/invoices")).StatusCode);

    [Fact]
    public async Task List_contains_only_current_tenant()
    {
        using var client = Client("tenant-a");
        var items = await client.GetFromJsonAsync<JsonElement>("/api/invoices");
        Assert.Single(items.EnumerateArray());
        Assert.Equal("A-100", items[0].GetProperty("number").GetString());
    }

    [Fact]
    public async Task Cross_tenant_identifier_is_not_disclosed()
    {
        using var client = Client("tenant-a");
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/invoices/00000000-0000-0000-0000-000000000002")).StatusCode);
    }

    [Fact]
    public void Cache_keys_are_tenant_scoped()
    {
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Assert.NotEqual(TenantCacheKey.For("tenant-a", "invoice", id), TenantCacheKey.For("tenant-b", "invoice", id));
    }

    [Fact]
    public async Task Quota_is_isolated_per_tenant()
    {
        using var tenantA = Client("tenant-a");
        using var tenantB = Client("tenant-b");
        Assert.Equal(HttpStatusCode.Created, (await Create(tenantA, "A-1")).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await Create(tenantA, "A-2")).StatusCode);
        Assert.Equal((HttpStatusCode)429, (await Create(tenantA, "A-3")).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await Create(tenantB, "B-1")).StatusCode);
    }

    private HttpClient Client(string tenant)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", $"user-{tenant}");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenant);
        return client;
    }
    private static Task<HttpResponseMessage> Create(HttpClient client, string number) => client.PostAsJsonAsync("/api/invoices", new { number, amount = 10m });
}
