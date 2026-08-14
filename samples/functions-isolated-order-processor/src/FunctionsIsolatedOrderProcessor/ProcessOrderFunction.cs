using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionsIsolatedOrderProcessor;

public sealed record OrderSubmitted(string EventId, string OrderId, decimal Amount);
public sealed record ProcessResult(string EventId, bool Applied);

public interface IOrderReceiptStore
{
    Task<bool> TryRecordAsync(OrderSubmitted order, CancellationToken cancellationToken);
}

public sealed class InMemoryOrderReceiptStore : IOrderReceiptStore
{
    private readonly ConcurrentDictionary<string, byte> receipts = new(StringComparer.Ordinal);

    public Task<bool> TryRecordAsync(OrderSubmitted order, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(receipts.TryAdd(order.EventId, 0));
    }
}

public sealed class OrderProcessor(IOrderReceiptStore receipts)
{
    public async Task<ProcessResult> ProcessAsync(string payload, CancellationToken cancellationToken)
    {
        var order = JsonSerializer.Deserialize<OrderSubmitted>(payload)
            ?? throw new InvalidDataException("Order event is required.");
        if (string.IsNullOrWhiteSpace(order.EventId) || string.IsNullOrWhiteSpace(order.OrderId) || order.Amount <= 0)
        {
            throw new InvalidDataException("EventId, OrderId, and a positive Amount are required.");
        }

        var applied = await receipts.TryRecordAsync(order, cancellationToken);
        return new(order.EventId, applied);
    }
}

public sealed class ProcessOrderFunction(OrderProcessor processor, ILogger<ProcessOrderFunction> logger)
{
    [Function(nameof(ProcessOrder))]
    public async Task ProcessOrder(
        [QueueTrigger("orders", Connection = "OrdersStorage")] string payload,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        var result = await processor.ProcessAsync(payload, cancellationToken);
        logger.LogInformation(
            "Order event {EventId} completed; side effect applied: {Applied}; invocation: {InvocationId}",
            result.EventId,
            result.Applied,
            context.InvocationId);
    }
}
