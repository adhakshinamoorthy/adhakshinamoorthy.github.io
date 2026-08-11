using Dapper;

namespace DapperOrderReporting.Persistence;

public sealed class DatabaseInitializer(SqliteConnectionFactory connections)
{
    private const string SchemaSql = """
        PRAGMA journal_mode = WAL;

        CREATE TABLE IF NOT EXISTS customers (
            id TEXT PRIMARY KEY,
            name TEXT NOT NULL CHECK (length(name) BETWEEN 1 AND 200),
            email TEXT NOT NULL COLLATE NOCASE UNIQUE
        );

        CREATE TABLE IF NOT EXISTS orders (
            id TEXT PRIMARY KEY,
            customer_id TEXT NOT NULL REFERENCES customers(id) ON DELETE RESTRICT,
            status TEXT NOT NULL CHECK (status IN ('Placed', 'Paid', 'Cancelled')),
            created_at_unix_seconds INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS order_items (
            id TEXT PRIMARY KEY,
            order_id TEXT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
            product_code TEXT NOT NULL,
            description TEXT NOT NULL CHECK (length(description) BETWEEN 1 AND 200),
            unit_price_cents INTEGER NOT NULL CHECK (unit_price_cents >= 0),
            quantity INTEGER NOT NULL CHECK (quantity BETWEEN 1 AND 100),
            UNIQUE (order_id, product_code)
        );

        CREATE INDEX IF NOT EXISTS ix_orders_status_created
            ON orders(status, created_at_unix_seconds DESC);
        CREATE INDEX IF NOT EXISTS ix_orders_customer ON orders(customer_id);
        CREATE INDEX IF NOT EXISTS ix_order_items_order ON order_items(order_id);
        """;

    private const string SeedSql = """
        INSERT OR IGNORE INTO customers (id, name, email)
        VALUES ('customer-portal-reader', 'Portal Reader', 'reader@example.com');

        INSERT OR IGNORE INTO orders (id, customer_id, status, created_at_unix_seconds)
        VALUES
            ('order-open', 'customer-portal-reader', 'Placed', 1786464000),
            ('order-paid', 'customer-portal-reader', 'Paid', 1786377600);

        INSERT OR IGNORE INTO order_items
            (id, order_id, product_code, description, unit_price_cents, quantity)
        VALUES
            ('line-open-1', 'order-open', 'BOOK-DOTNET', '.NET architecture handbook', 4995, 1),
            ('line-open-2', 'order-open', 'COURSE-SQL', 'SQL performance workshop', 1950, 2),
            ('line-paid-1', 'order-paid', 'LAB-DAPPER', 'Dapper practice lab', 2500, 1);
        """;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await connections.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(SchemaSql, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(SeedSql, cancellationToken: cancellationToken));
    }
}
