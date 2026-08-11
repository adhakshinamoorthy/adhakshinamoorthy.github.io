using EfCoreOrderManagement.Application;
using EfCoreOrderManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace EfCoreOrderManagement.Tests;

public sealed class PostgreSqlProviderTests
{
    [PostgresFact]
    public async Task Model_and_projection_work_on_PostgreSQL()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("orders")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;
        await using var db = new OrderingDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await DatabaseSeeder.SeedAsync(db);

        var summary = Assert.Single(await new OrderQueries(db).GetOpenOrdersAsync(0, 10));
        Assert.Equal(88.95m, summary.Total);
    }
}

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("EFCORE_RUN_POSTGRES_TESTS"), "1", StringComparison.Ordinal))
        {
            Skip = "Set EFCORE_RUN_POSTGRES_TESTS=1 and start Docker to run the PostgreSQL provider test.";
        }
    }
}
