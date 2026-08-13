using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiSecurityOwaspOrders.Tests;

public sealed class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public SecurityTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact] public async Task Anonymous_is_rejected() => Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/orders")).StatusCode);

    [Fact]
    public async Task Cross_subject_identifier_is_hidden()
    {
        using var request = Request(HttpMethod.Get, "/api/orders/11111111-1111-1111-1111-111111111111", "mallory", "north");
        Assert.Equal(HttpStatusCode.NotFound, (await _client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task Page_size_is_bounded()
    {
        using var request = Request(HttpMethod.Get, "/api/orders?limit=1000", "alice", "north");
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task Response_omits_internal_risk_score()
    {
        using var request = Request(HttpMethod.Get, "/api/orders/11111111-1111-1111-1111-111111111111", "alice", "north");
        var body = await (await _client.SendAsync(request)).Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.TryGetProperty("internalRiskScore", out _));
    }

    [Fact]
    public async Task Arbitrary_outbound_destination_is_rejected()
    {
        using var request = Request(HttpMethod.Get, "/api/inventory-probe?destination=http://169.254.169.254/latest/meta-data", "alice", "north");
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.SendAsync(request)).StatusCode);
    }

    private static HttpRequestMessage Request(HttpMethod method, string path, string subject, string tenant)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-Subject", subject);
        request.Headers.Add("X-Tenant", tenant);
        return request;
    }
}
