using CSharpLanguageWorkbench.Application;
using CSharpLanguageWorkbench.Domain;
using CSharpLanguageWorkbench.Infrastructure;
using Xunit;

namespace CSharpLanguageWorkbench.Tests;

public sealed class OrderProcessorTests
{
    [Fact]
    public void Decide_SendsLargePendingOrderToManualReview()
    {
        var processor = new OrderProcessor(1_000m);
        var order = CreateOrder(OrderStatus.Pending, 1_250m);

        Assert.Equal(NextAction.ManualReview, processor.Decide(order));
    }

    [Fact]
    public void Decide_FulfilsPaidOrder()
    {
        var processor = new OrderProcessor(1_000m);
        var order = CreateOrder(OrderStatus.Paid, 50m);

        Assert.Equal(NextAction.Fulfil, processor.Decide(order));
    }

    [Fact]
    public void Total_AggregatesOrderLines()
    {
        var order = new Order(
            Guid.NewGuid(),
            "customer@example.com",
            OrderStatus.Pending,
            [
                new OrderLine("ONE", 2, Money.From(12.50m, "usd")),
                new OrderLine("TWO", 1, Money.From(5m, "USD"))
            ]);

        Assert.Equal(Money.From(30m, "USD"), order.Total);
    }

    [Fact]
    public async Task ProcessAsync_SortsResultsAndPreservesNullableIntent()
    {
        var processor = new OrderProcessor(1_000m);
        var smaller = CreateOrder(OrderStatus.Pending, 25m, promotionCode: null);
        var larger = CreateOrder(OrderStatus.Pending, 1_500m, promotionCode: "SAVE10");

        var decisions = await processor.ProcessAsync(
            InMemoryOrderSource.ReadAsync([smaller, larger]));

        Assert.Collection(
            decisions,
            first =>
            {
                Assert.Equal(larger.Id, first.OrderId);
                Assert.True(first.PromotionApplied);
            },
            second =>
            {
                Assert.Equal(smaller.Id, second.OrderId);
                Assert.False(second.PromotionApplied);
            });
    }

    [Fact]
    public async Task ProcessAsync_ObservesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var processor = new OrderProcessor(1_000m);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            processor.ProcessAsync(
                InMemoryOrderSource.ReadAsync([CreateOrder(OrderStatus.Pending, 10m)]),
                cancellation.Token));
    }

    [Fact]
    public void Money_RejectsMixedCurrencies()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Money.From(10m, "USD") + Money.From(10m, "EUR"));
    }

    private static Order CreateOrder(
        OrderStatus status,
        decimal amount,
        string? promotionCode = null) =>
        new(
            Guid.NewGuid(),
            "customer@example.com",
            status,
            [new OrderLine("SKU", 1, Money.From(amount, "USD"))],
            promotionCode);
}
