using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AuthenticationAuthorizationDocuments.Tests;

public sealed class AuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string AliceDocument = "/api/documents/11111111-1111-1111-1111-111111111111";
    private readonly HttpClient _client;

    public AuthorizationTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Anonymous_request_is_challenged()
    {
        var response = await _client.GetAsync(AliceDocument);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Owner_can_read_document()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, AliceDocument);
        request.Headers.Add("X-User", "alice");
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Non_owner_receives_not_found_to_avoid_disclosure()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, AliceDocument);
        request.Headers.Add("X-User", "bob");
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Administrator_can_read_document()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, AliceDocument);
        request.Headers.Add("X-User", "carol");
        request.Headers.Add("X-Role", "admin");
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_requires_write_scope()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/documents");
        request.Headers.Add("X-User", "alice");
        request.Content = JsonContent.Create(new { title = "Draft", content = "Text" });
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
