using DapperOrderReporting.Persistence;

namespace DapperOrderReporting.Tests;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"dapper-orders-{Guid.NewGuid():N}");

    public TestDatabase()
    {
        Directory.CreateDirectory(_directory);
        Connections = new SqliteConnectionFactory(
            $"Data Source={Path.Combine(_directory, "orders.db")};Pooling=False");
    }

    public SqliteConnectionFactory Connections { get; }

    public async Task InitializeAsync() =>
        await new DatabaseInitializer(Connections).InitializeAsync();

    public ValueTask DisposeAsync()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
        return ValueTask.CompletedTask;
    }
}
