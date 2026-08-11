namespace DapperOrderReporting.Models;

public enum OrderSort
{
    Newest,
    Oldest,
    HighestValue
}

public sealed record OrderSearch(
    string? Status = null,
    long? MinimumTotalCents = null,
    OrderSort Sort = OrderSort.Newest,
    int Page = 0,
    int PageSize = 20);

public sealed record OrderSummary(
    string Id,
    string CustomerName,
    string Status,
    long CreatedAtUnixSeconds,
    long LineCount,
    long TotalCents);

public sealed class OrderDetails
{
    public required string Id { get; init; }

    public required string CustomerName { get; init; }

    public required string Status { get; init; }

    public long CreatedAtUnixSeconds { get; init; }

    public List<OrderLine> Lines { get; } = [];

    public long TotalCents => Lines.Sum(line => line.UnitPriceCents * line.Quantity);
}

public sealed record OrderLine(
    string LineId,
    string ProductCode,
    string Description,
    long UnitPriceCents,
    long Quantity);

public sealed record StatusCount(string Status, long OrderCount);

public sealed record CustomerSpend(string CustomerName, long TotalCents);

public sealed record OrderDashboard(
    IReadOnlyList<StatusCount> Statuses,
    IReadOnlyList<CustomerSpend> TopCustomers);

public sealed record NewOrderLine(
    string ProductCode,
    string Description,
    long UnitPriceCents,
    int Quantity);

public sealed record NewOrder(
    string CustomerId,
    IReadOnlyList<NewOrderLine> Lines);
