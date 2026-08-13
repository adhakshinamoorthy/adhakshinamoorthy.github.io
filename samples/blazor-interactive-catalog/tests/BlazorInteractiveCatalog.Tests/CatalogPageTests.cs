using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BlazorInteractiveCatalog.Tests;

public sealed class CatalogPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CatalogPageTests(WebApplicationFactory<Program> factory)
    {
        var testFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureLogging(logging => logging.ClearProviders()));
        _client = testFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Home_ReturnsPrerenderedCatalogAndInteractiveBootstrap()
    {
        using var response = await _client.GetAsync("/", CancellationToken.None);
        var html = await response.Content.ReadAsStringAsync(CancellationToken.None);

        response.EnsureSuccessStatusCode();
        Assert.Contains("Product catalog", html, StringComparison.Ordinal);
        Assert.Contains("Architecture Field Guide", html, StringComparison.Ordinal);
        Assert.Contains("aria-live=\"polite\"", html, StringComparison.Ordinal);
        Assert.Contains("_framework/blazor.web.js", html, StringComparison.Ordinal);
        Assert.Contains("\"type\":\"server\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Home_AddsBaselineSecurityHeaders()
    {
        using var response = await _client.GetAsync("/", CancellationToken.None);

        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("strict-origin-when-cross-origin", Assert.Single(response.Headers.GetValues("Referrer-Policy")));
    }

    [Fact]
    public async Task UnknownRoute_ReturnsNotFound()
    {
        using var response = await _client.GetAsync("/missing", CancellationToken.None);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
