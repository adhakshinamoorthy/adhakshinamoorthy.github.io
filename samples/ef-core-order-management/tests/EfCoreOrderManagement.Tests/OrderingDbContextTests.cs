using EfCoreOrderManagement.Application;
using EfCoreOrderManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EfCoreOrderManagement.Tests;

public sealed class OrderingDbContextTests
{
    [Fact]
    public async Task Migrations_create_a_queryable_schema()
    {
        await using var database = new SqliteDatabase();
        await database.MigrateAsync();
        await using var db = database.CreateContext();

        var applied = await db.Database.GetAppliedMigrationsAsync();
        var tables = await db.Database.SqlQueryRaw<string>(
            "SELECT name AS Value FROM sqlite_master WHERE type = 'table' AND name IN ('customers', 'orders', 'order_items')")
            .ToListAsync();

        Assert.Single(applied);
        Assert.Equal(3, tables.Count);
    }

    [Fact]
    public async Task Projected_query_is_bounded_and_does_not_track_entities()
    {
        await using var database = new SqliteDatabase();
        await database.MigrateAsync();
        await using (var seed = database.CreateContext()) await DatabaseSeeder.SeedAsync(seed);
        await using var db = database.CreateContext();

        var summaries = await new OrderQueries(db).GetOpenOrdersAsync(page: 0, pageSize: 10);

        var summary = Assert.Single(summaries);
        Assert.Equal("Portal Reader", summary.CustomerName);
        Assert.Equal(88.95m, summary.Total);
        Assert.Empty(db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task Unique_email_constraint_is_enforced_by_the_database()
    {
        await using var database = new SqliteDatabase();
        await database.MigrateAsync();
        await using var db = database.CreateContext();
        db.Customers.Add(Customer.Create("reader@example.com", "First"));
        db.Customers.Add(Customer.Create("READER@example.com", "Second"));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Concurrency_token_rejects_a_stale_update()
    {
        await using var database = new SqliteDatabase();
        await database.MigrateAsync();
        await using (var seed = database.CreateContext()) await DatabaseSeeder.SeedAsync(seed);
        await using var first = database.CreateContext();
        await using var second = database.CreateContext();

        var firstOrder = await first.Orders.SingleAsync();
        var staleOrder = await second.Orders.SingleAsync();
        firstOrder.MarkPaid();
        await first.SaveChangesAsync();
        staleOrder.MarkPaid();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }
}
