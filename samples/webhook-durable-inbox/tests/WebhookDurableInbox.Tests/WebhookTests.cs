using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace WebhookDurableInbox.Tests;

public sealed class WebhookTests : IAsyncLifetime
{
    private const string Secret = "integration-test-secret";
    private readonly string _inboxPath = Path.Combine(Path.GetTempPath(), $"webhook-inbox-{Guid.NewGuid():N}.json");
    private WebApplicationFactory<Program>? _factory;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WebhookSecret"] = Secret,
                ["WebhookInboxPath"] = _inboxPath
            }));
        });
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        if (_factory is not null) await _factory.DisposeAsync();
        if (File.Exists(_inboxPath)) File.Delete(_inboxPath);
        if (File.Exists(_inboxPath + ".tmp")) File.Delete(_inboxPath + ".tmp");
    }

    [Fact]
    public async Task Valid_delivery_is_persisted_and_processed()
    {
        var response = await Send("delivery-1", "order.paid", "{\"orderId\":\"ORD-1\"}");
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var status = await WaitForStatus("delivery-1", "completed");
        Assert.Equal(1, status.GetProperty("attempts").GetInt32());
        Assert.True(File.Exists(_inboxPath));
    }

    [Fact]
    public async Task Invalid_signature_is_rejected_before_persistence()
    {
        var response = await Send("delivery-2", "order.paid", "{}", "sha256=" + new string('0', 64));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/webhooks/deliveries/delivery-2")).StatusCode);
    }

    [Fact]
    public async Task Duplicate_delivery_is_acknowledged_without_second_processing()
    {
        var first = await Send("delivery-3", "order.paid", "{\"orderId\":\"ORD-3\"}");
        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        await WaitForStatus("delivery-3", "completed");
        var duplicate = await Send("delivery-3", "order.paid", "{\"orderId\":\"ORD-3\"}");
        Assert.Equal(HttpStatusCode.OK, duplicate.StatusCode);
        var status = await ReadStatus("delivery-3");
        Assert.Equal(1, status.GetProperty("attempts").GetInt32());
    }

    [Fact]
    public async Task Signature_uses_exact_utf8_bytes()
    {
        var body = "{\"message\":\"Paid ✓\"}";
        Assert.Equal(HttpStatusCode.Accepted, (await Send("delivery-unicode", "order.paid", body)).StatusCode);
        await WaitForStatus("delivery-unicode", "completed");
    }

    private async Task<HttpResponseMessage> Send(string id, string eventType, string body, string? signature = null)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        signature ??= "sha256=" + Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(Secret), bytes)).ToLowerInvariant();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/orders");
        request.Headers.Add("X-Delivery-Id", id);
        request.Headers.Add("X-Event-Type", eventType);
        request.Headers.Add("X-Signature-256", signature);
        request.Content = new ByteArrayContent(bytes);
        request.Content.Headers.ContentType = new("application/json") { CharSet = "utf-8" };
        return await _client.SendAsync(request);
    }

    private async Task<JsonElement> WaitForStatus(string id, string expected)
    {
        for (var attempt = 0; attempt < 40; attempt++)
        {
            var status = await ReadStatus(id);
            if (string.Equals(status.GetProperty("status").GetString(), expected, StringComparison.OrdinalIgnoreCase)) return status;
            await Task.Delay(50);
        }
        throw new TimeoutException($"Delivery {id} did not reach {expected}.");
    }

    private async Task<JsonElement> ReadStatus(string id)
    {
        var json = await _client.GetStringAsync($"/webhooks/deliveries/{id}");
        return JsonSerializer.Deserialize<JsonElement>(json);
    }
}
