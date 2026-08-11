using Dapper;
using Microsoft.Data.Sqlite;

namespace DapperOrderReporting.Persistence;

public sealed class SqliteConnectionFactory(string connectionString)
{
    public async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            "PRAGMA foreign_keys = ON;",
            cancellationToken: cancellationToken));
        return connection;
    }
}
