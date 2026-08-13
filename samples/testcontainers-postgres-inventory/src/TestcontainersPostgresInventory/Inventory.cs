using Npgsql;

namespace TestcontainersPostgresInventory;

public sealed record InventoryItem
{
    public InventoryItem(string sku, int available)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegative(available);
        Sku = sku;
        Available = available;
    }

    public string Sku { get; }
    public int Available { get; }
}

public sealed class InventoryRepository(string connectionString) : IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource = NpgsqlDataSource.Create(connectionString);

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken = default)
    {
        await using var command = _dataSource.CreateCommand(
            """
            CREATE TABLE IF NOT EXISTS inventory (
                sku text PRIMARY KEY,
                available integer NOT NULL CHECK (available >= 0)
            );
            """);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpsertAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        await using var command = _dataSource.CreateCommand(
            """
            INSERT INTO inventory (sku, available) VALUES ($1, $2)
            ON CONFLICT (sku) DO UPDATE SET available = EXCLUDED.available;
            """);
        command.Parameters.AddWithValue(item.Sku);
        command.Parameters.AddWithValue(item.Available);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<InventoryItem?> FindAsync(string sku, CancellationToken cancellationToken = default)
    {
        await using var command = _dataSource.CreateCommand(
            "SELECT sku, available FROM inventory WHERE sku = $1;");
        command.Parameters.AddWithValue(sku);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new InventoryItem(reader.GetString(0), reader.GetInt32(1))
            : null;
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await using var command = _dataSource.CreateCommand("TRUNCATE TABLE inventory;");
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public ValueTask DisposeAsync() => _dataSource.DisposeAsync();
}

public static class PostgresTestEnvironment
{
    public const string Image = "postgres:16.4-alpine";
}
