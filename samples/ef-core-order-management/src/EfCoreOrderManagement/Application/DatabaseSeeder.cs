using EfCoreOrderManagement.Domain;
using EfCoreOrderManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCoreOrderManagement.Application;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(OrderingDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Orders.AnyAsync(cancellationToken)) return;

        var customer = Customer.Create("reader@example.com", "Portal Reader");
        var order = Order.Place(
            customer.Id,
            new Address("1 Learning Lane", "Chennai", "IN"),
            [
                new OrderLineInput("BOOK-EF10", "Practical EF Core", 49.95m, 1),
                new OrderLineInput("LAB-SQL", "Query diagnostics lab", 19.50m, 2)
            ],
            new DateTime(2026, 8, 11, 12, 0, 0, DateTimeKind.Utc));

        db.Add(customer);
        db.Add(order);
        await db.SaveChangesAsync(cancellationToken);
    }
}
