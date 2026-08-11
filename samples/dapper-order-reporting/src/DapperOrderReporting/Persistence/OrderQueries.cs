using Dapper;
using DapperOrderReporting.Models;

namespace DapperOrderReporting.Persistence;

public sealed class OrderQueries(SqliteConnectionFactory connections)
{
    private const string SearchSql = """
        SELECT
            o.id AS Id,
            c.name AS CustomerName,
            o.status AS Status,
            o.created_at_unix_seconds AS CreatedAtUnixSeconds,
            COUNT(i.id) AS LineCount,
            COALESCE(SUM(i.unit_price_cents * i.quantity), 0) AS TotalCents
        FROM orders o
        JOIN customers c ON c.id = o.customer_id
        LEFT JOIN order_items i ON i.order_id = o.id
        WHERE (@Status IS NULL OR o.status = @Status)
        GROUP BY o.id, c.name, o.status, o.created_at_unix_seconds
        HAVING (@MinimumTotalCents IS NULL OR COALESCE(SUM(i.unit_price_cents * i.quantity), 0) >= @MinimumTotalCents)
        ORDER BY {0}
        LIMIT @PageSize OFFSET @Offset;
        """;

    private static readonly IReadOnlyDictionary<OrderSort, string> SortClauses =
        new Dictionary<OrderSort, string>
        {
            [OrderSort.Newest] = "o.created_at_unix_seconds DESC, o.id",
            [OrderSort.Oldest] = "o.created_at_unix_seconds ASC, o.id",
            [OrderSort.HighestValue] = "TotalCents DESC, o.id"
        };

    public async Task<IReadOnlyList<OrderSummary>> SearchAsync(
        OrderSearch search,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(search.Page);
        if (search.PageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(search), "Page size must be between 1 and 100.");
        }

        if (!SortClauses.TryGetValue(search.Sort, out var orderBy))
        {
            throw new ArgumentOutOfRangeException(nameof(search), "Unknown order sort.");
        }

        var sql = string.Format(System.Globalization.CultureInfo.InvariantCulture, SearchSql, orderBy);
        var parameters = new
        {
            search.Status,
            search.MinimumTotalCents,
            search.PageSize,
            Offset = checked(search.Page * search.PageSize)
        };

        await using var connection = await connections.OpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        return (await connection.QueryAsync<OrderSummary>(command)).AsList();
    }

    public async Task<OrderDetails?> GetWithLinesAsync(string orderId)
    {
        const string sql = """
            SELECT
                o.id AS Id,
                c.name AS CustomerName,
                o.status AS Status,
                o.created_at_unix_seconds AS CreatedAtUnixSeconds,
                i.id AS LineId,
                i.product_code AS ProductCode,
                i.description AS Description,
                i.unit_price_cents AS UnitPriceCents,
                i.quantity AS Quantity
            FROM orders o
            JOIN customers c ON c.id = o.customer_id
            LEFT JOIN order_items i ON i.order_id = o.id
            WHERE o.id = @OrderId
            ORDER BY i.id;
            """;

        await using var connection = await connections.OpenAsync();
        var lookup = new Dictionary<string, OrderDetails>(StringComparer.Ordinal);
        await connection.QueryAsync<OrderDetails, OrderLine, OrderDetails>(
            sql,
            (order, line) =>
            {
                if (!lookup.TryGetValue(order.Id, out var current))
                {
                    current = order;
                    lookup.Add(order.Id, current);
                }

                if (line is not null) current.Lines.Add(line);
                return current;
            },
            new { OrderId = orderId },
            splitOn: "LineId");

        return lookup.Values.SingleOrDefault();
    }

    public async Task<OrderDashboard> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT status AS Status, COUNT(*) AS OrderCount
            FROM orders
            GROUP BY status
            ORDER BY status;

            SELECT c.name AS CustomerName,
                   SUM(i.unit_price_cents * i.quantity) AS TotalCents
            FROM customers c
            JOIN orders o ON o.customer_id = c.id
            JOIN order_items i ON i.order_id = o.id
            GROUP BY c.id, c.name
            ORDER BY TotalCents DESC
            LIMIT 5;
            """;

        await using var connection = await connections.OpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        await using var grids = await connection.QueryMultipleAsync(command);
        var statuses = (await grids.ReadAsync<StatusCount>()).AsList();
        var customers = (await grids.ReadAsync<CustomerSpend>()).AsList();
        return new OrderDashboard(statuses, customers);
    }
}
