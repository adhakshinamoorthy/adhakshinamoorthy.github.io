using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EfCoreOrderManagement.Persistence;

public sealed class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args)
    {
        var databasePath = Path.GetFullPath(Path.Combine("artifacts", "orders.db"));
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        return new OrderingDbContext(options);
    }
}
