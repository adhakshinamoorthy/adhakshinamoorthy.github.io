using FunctionsIsolatedOrderProcessor;
using Xunit;

namespace FunctionsIsolatedOrderProcessor.Tests;

public sealed class OrderProcessorTests
{
    [Fact]
    public async Task Duplicate_event_is_acknowledged_without_repeating_effect()
    {
        var processor = new OrderProcessor(new InMemoryOrderReceiptStore());
        const string payload = "{\"EventId\":\"event-1\",\"OrderId\":\"order-1\",\"Amount\":42.50}";

        Assert.True((await processor.ProcessAsync(payload, CancellationToken.None)).Applied);
        Assert.False((await processor.ProcessAsync(payload, CancellationToken.None)).Applied);
    }

    [Theory]
    [InlineData("{}")] [InlineData("null")] [InlineData("{\"EventId\":\"e\",\"OrderId\":\"o\",\"Amount\":0}")]
    public async Task Invalid_event_is_rejected(string payload)
    {
        var processor = new OrderProcessor(new InMemoryOrderReceiptStore());
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            processor.ProcessAsync(payload, CancellationToken.None));
    }
}
