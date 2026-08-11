using EfCoreOrderManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCoreOrderManagement.Tests;

internal sealed class SqliteDatabase : IAsyncDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"efcore-orders-{Guid.NewGuid():N}");

    public SqliteDatabase()
    {
        Directory.CreateDirectory(_directory);
        ConnectionString = $"Data Source={Path.Combine(_directory, "orders.db")};Pooling=False";
    }

    public string ConnectionString { get; }

    public OrderingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseSqlite(ConnectionString)
            .EnableDetailedErrors()
            .Options;

        return new OrderingDbContext(options);
    }

    public async Task MigrateAsync()
    {
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public ValueTask DisposeAsync()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
        return ValueTask.CompletedTask;
    }
}
