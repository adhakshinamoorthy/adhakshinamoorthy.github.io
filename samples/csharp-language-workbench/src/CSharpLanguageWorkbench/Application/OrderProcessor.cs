using CSharpLanguageWorkbench.Domain;

namespace CSharpLanguageWorkbench.Application;

public enum NextAction
{
    CapturePayment,
    ManualReview,
    Fulfil,
    NoAction
}

public sealed record OrderDecision(
    Guid OrderId,
    string CustomerEmail,
    Money Total,
    NextAction Action,
    bool PromotionApplied);

public sealed class OrderProcessor(decimal manualReviewThreshold)
{
    public NextAction Decide(Order order) => order switch
    {
        { Status: OrderStatus.Pending, Total.Amount: var total } when total >= manualReviewThreshold
            => NextAction.ManualReview,
        { Status: OrderStatus.Pending } => NextAction.CapturePayment,
        { Status: OrderStatus.Paid } => NextAction.Fulfil,
        _ => NextAction.NoAction
    };

    public async Task<IReadOnlyList<OrderDecision>> ProcessAsync(
        IAsyncEnumerable<Order> orders,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orders);

        var decisions = new List<OrderDecision>();
        await foreach (var order in orders.WithCancellation(cancellationToken))
        {
            Validate(order);
            decisions.Add(new OrderDecision(
                order.Id,
                order.CustomerEmail,
                order.Total,
                Decide(order),
                order.HasPromotion));
        }

        return decisions
            .OrderByDescending(decision => decision.Total.Amount)
            .ThenBy(decision => decision.OrderId)
            .ToArray();
    }

    private static void Validate(Order order)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(order.CustomerEmail);
        ArgumentOutOfRangeException.ThrowIfZero(order.Lines.Count);

        if (order.Lines.Any(line => line.Quantity <= 0))
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Line quantities must be positive.");
        }
    }
}
