using CSharpLanguageWorkbench.Application;
using CSharpLanguageWorkbench.Domain;
using CSharpLanguageWorkbench.Infrastructure;

var orders = new Order[]
{
    new(
        Guid.Parse("10000000-0000-0000-0000-000000000001"),
        "ada@example.com",
        OrderStatus.Pending,
        [new("LAPTOP", 1, Money.From(1_250m, "USD"))],
        "ARCHITECT10"),
    new(
        Guid.Parse("10000000-0000-0000-0000-000000000002"),
        "grace@example.com",
        OrderStatus.Paid,
        [new("BOOK", 2, Money.From(45m, "USD"))])
};

var processor = new OrderProcessor(manualReviewThreshold: 1_000m);
var decisions = await processor.ProcessAsync(InMemoryOrderSource.ReadAsync(orders));

foreach (var decision in decisions)
{
    Console.WriteLine(
        "{0}: {1} {2:0.00} -> {3} (promotion: {4})",
        decision.CustomerEmail,
        decision.Total.Currency,
        decision.Total.Amount,
        decision.Action,
        decision.PromotionApplied);
}
