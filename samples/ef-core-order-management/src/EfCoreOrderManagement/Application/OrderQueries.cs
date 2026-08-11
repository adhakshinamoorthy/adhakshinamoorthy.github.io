using EfCoreOrderManagement.Domain;
using EfCoreOrderManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCoreOrderManagement.Application;

public sealed record OrderSummary(Guid Id, DateTime CreatedAtUtc, string CustomerName, int LineCount, decimal Total);

public sealed class OrderQueries(OrderingDbContext db)
{
    public IQueryable<OrderSummary> BuildOpenOrdersQuery(int page, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, 100);

        return db.Orders
            .TagWith("Open order summaries")
            .AsNoTracking()
            .Where(order => order.Status == OrderStatus.Placed)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ThenBy(order => order.Id)
            .Select(order => new OrderSummary(
                order.Id,
                order.CreatedAtUtc,
                order.Customer.Name,
                order.Items.Count,
                order.Items.Sum(item => item.UnitPrice * item.Quantity)))
            .Skip(page * pageSize)
            .Take(pageSize);
    }

    public async Task<IReadOnlyList<OrderSummary>> GetOpenOrdersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        await BuildOpenOrdersQuery(page, pageSize).ToListAsync(cancellationToken);
}
