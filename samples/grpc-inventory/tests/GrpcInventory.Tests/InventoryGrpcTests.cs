using Grpc.Core;
using Grpc.Net.Client;
using GrpcInventory.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GrpcInventory.Tests;

public sealed class InventoryGrpcTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Inventory.InventoryClient _client;
    private static readonly Metadata Credentials = new() { { "x-api-key", "local-demo-key" } };

    public InventoryGrpcTests(WebApplicationFactory<Program> factory)
    {
        var httpClient = factory.CreateDefaultClient(new ResponseVersionHandler());
        _client = new Inventory.InventoryClient(GrpcChannel.ForAddress(httpClient.BaseAddress!, new GrpcChannelOptions { HttpClient = httpClient }));
    }

    [Fact]
    public async Task Unary_lookup_returns_typed_stock()
    {
        var reply = await _client.GetStockAsync(new GetStockRequest { Sku = "sku-red" }, Credentials);
        Assert.Equal("SKU-RED", reply.Sku);
        Assert.Equal(12, reply.Available);
    }

    [Fact]
    public async Task Missing_credentials_are_unauthenticated()
    {
        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _client.GetStockAsync(new GetStockRequest { Sku = "SKU-RED" }));
        Assert.Equal(StatusCode.Unauthenticated, exception.StatusCode);
    }

    [Fact]
    public async Task Invalid_input_uses_grpc_status()
    {
        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _client.GetStockAsync(new GetStockRequest { Sku = "x" }, Credentials));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task Server_stream_returns_requested_known_items()
    {
        var request = new WatchStockRequest();
        request.Skus.AddRange(["SKU-RED", "SKU-BLUE", "UNKNOWN"]);
        using var call = _client.WatchStock(request, Credentials);
        var replies = new List<StockReply>();
        await foreach (var reply in call.ResponseStream.ReadAllAsync()) replies.Add(reply);
        Assert.Equal(["SKU-RED", "SKU-BLUE"], replies.Select(item => item.Sku));
    }

    private sealed class ResponseVersionHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Version = new Version(2, 0);
            request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
            return base.SendAsync(request, cancellationToken);
        }
    }
}
