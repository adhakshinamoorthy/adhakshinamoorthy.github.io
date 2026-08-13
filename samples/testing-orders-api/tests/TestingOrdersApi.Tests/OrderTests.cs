using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TestingOrdersApi;
using Xunit;

public sealed class OrderUnitTests
{
    [Fact]
    public void Create_calculates_total_and_uses_controlled_time()
    {
        var now = new DateTimeOffset(2026, 8, 13, 12, 0, 0, TimeSpan.Zero);
        var service = new OrderService(new FixedTimeProvider(now));

        var order = service.Create(new("customer-1", 3, 4.50m));

        Assert.Equal(13.50m, order.Total);
        Assert.Equal(now, order.CreatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_quantity(int quantity)
    {
        var service = new OrderService(TimeProvider.System);
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Create(new("customer-1", quantity, 1m)));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

public sealed class OrderHttpTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderHttpTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Critical_journey_creates_then_reads_order()
    {
        var create = await _client.PostAsJsonAsync("/orders", new CreateOrderRequest("customer-2", 2, 6m));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var receipt = await create.Content.ReadFromJsonAsync<OrderReceipt>();
        Assert.NotNull(receipt);
        var read = await _client.GetAsync($"/orders/{receipt.Id}");
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(receipt, await read.Content.ReadFromJsonAsync<OrderReceipt>());
    }

    [Fact]
    public async Task Invalid_order_returns_validation_problem_contract()
    {
        var response = await _client.PostAsJsonAsync("/orders", new CreateOrderRequest("customer-2", 0, 6m));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Missing_order_returns_not_found()
    {
        var response = await _client.GetAsync($"/orders/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
