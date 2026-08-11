using Dapper;
using DapperOrderReporting.Models;
using DapperOrderReporting.Persistence;
using Microsoft.Data.Sqlite;
using Xunit;

namespace DapperOrderReporting.Tests;

public sealed class OrderDataAccessTests
{
    [Fact]
    public async Task Search_projects_filters_and_bounds_results()
    {
        await using var database = new TestDatabase();
        await database.InitializeAsync();
        var queries = new OrderQueries(database.Connections);

        var results = await queries.SearchAsync(new OrderSearch(
            Status: "Placed",
            MinimumTotalCents: 5000,
            Sort: OrderSort.HighestValue,
            PageSize: 1));

        var order = Assert.Single(results);
        Assert.Equal("order-open", order.Id);
        Assert.Equal(2, order.LineCount);
        Assert.Equal(8895, order.TotalCents);
    }

    [Fact]
    public async Task Pagination_uses_a_stable_tie_breaker()
    {
        await using var database = new TestDatabase();
        await database.InitializeAsync();
        var queries = new OrderQueries(database.Connections);

        var first = Assert.Single(await queries.SearchAsync(new OrderSearch(PageSize: 1)));
        var second = Assert.Single(await queries.SearchAsync(new OrderSearch(Page: 1, PageSize: 1)));

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal("order-open", first.Id);
        Assert.Equal("order-paid", second.Id);
    }

    [Fact]
    public async Task Multi_mapping_builds_one_order_with_its_lines()
    {
        await using var database = new TestDatabase();
        await database.InitializeAsync();
        var queries = new OrderQueries(database.Connections);

        var order = await queries.GetWithLinesAsync("order-open");

        Assert.NotNull(order);
        Assert.Equal("Portal Reader", order.CustomerName);
        Assert.Equal(2, order.Lines.Count);
        Assert.Equal(8895, order.TotalCents);
    }

    [Fact]
    public async Task Query_multiple_reads_a_dashboard_in_one_round_trip()
    {
        await using var database = new TestDatabase();
        await database.InitializeAsync();
        var dashboard = await new OrderQueries(database.Connections).GetDashboardAsync();

        Assert.Equal(2, dashboard.Statuses.Count);
        var customer = Assert.Single(dashboard.TopCustomers);
        Assert.Equal("Portal Reader", customer.CustomerName);
        Assert.Equal(11395, customer.TotalCents);
    }

    [Fact]
    public async Task Transaction_commits_an_order_and_all_lines()
    {
        await using var database = new TestDatabase();
        await database.InitializeAsync();
        var writer = new OrderWriter(database.Connections);

        var orderId = await writer.CreateAsync(new NewOrder(
            "customer-portal-reader",
            [new NewOrderLine("LAB-SQL", "SQL practice lab", 1800, 2)]));

        var created = await new OrderQueries(database.Connections).GetWithLinesAsync(orderId);
        Assert.NotNull(created);
        Assert.Single(created.Lines);
        Assert.Equal(3600, created.TotalCents);
    }

    [Fact]
    public async Task Constraint_failure_rolls_back_the_order_header()
    {
        await using var database = new TestDatabase();
        await database.InitializeAsync();
        var writer = new OrderWriter(database.Connections);
        var duplicateLines = new NewOrder(
            "customer-portal-reader",
            [
                new NewOrderLine("DUPLICATE", "First line", 1000, 1),
                new NewOrderLine("DUPLICATE", "Second line", 1200, 1)
            ]);

        await Assert.ThrowsAsync<SqliteException>(() => writer.CreateAsync(duplicateLines));

        await using var connection = await database.Connections.OpenAsync();
        var count = await connection.QuerySingleAsync<long>("SELECT COUNT(*) FROM orders;");
        Assert.Equal(2, count);
    }
}
