using Dapper;
using DapperOrderReporting.Models;

namespace DapperOrderReporting.Persistence;

public sealed class OrderWriter(SqliteConnectionFactory connections)
{
    public async Task<string> CreateAsync(NewOrder order, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(order.CustomerId);
        if (order.Lines.Count is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "An order must contain between 1 and 100 lines.");
        }

        foreach (var line in order.Lines)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(line.ProductCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(line.Description);
            if (line.UnitPriceCents < 0 || line.Quantity is < 1 or > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(order), "Line price and quantity are outside the allowed range.");
            }
        }

        var orderId = $"order-{Guid.NewGuid():N}";
        await using var connection = await connections.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string insertOrder = """
                INSERT INTO orders (id, customer_id, status, created_at_unix_seconds)
                VALUES (@Id, @CustomerId, 'Placed', @CreatedAtUnixSeconds);
                """;
            await connection.ExecuteAsync(new CommandDefinition(
                insertOrder,
                new
                {
                    Id = orderId,
                    order.CustomerId,
                    CreatedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                },
                transaction,
                cancellationToken: cancellationToken));

            const string insertLine = """
                INSERT INTO order_items
                    (id, order_id, product_code, description, unit_price_cents, quantity)
                VALUES (@Id, @OrderId, @ProductCode, @Description, @UnitPriceCents, @Quantity);
                """;
            foreach (var line in order.Lines)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    insertLine,
                    new
                    {
                        Id = $"line-{Guid.NewGuid():N}",
                        OrderId = orderId,
                        line.ProductCode,
                        line.Description,
                        line.UnitPriceCents,
                        line.Quantity
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            await transaction.CommitAsync(cancellationToken);
            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
