using EfCoreOrderManagement.Application;
using EfCoreOrderManagement.Persistence;
using Microsoft.EntityFrameworkCore;

var databaseArgument = args
    .Select((value, index) => (value, index))
    .FirstOrDefault(item => item.value == "--database");
var databasePath = databaseArgument.value is not null && databaseArgument.index + 1 < args.Length
    ? args[databaseArgument.index + 1]
    : Path.Combine("artifacts", "orders.db");
databasePath = Path.GetFullPath(databasePath);
Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

var options = new DbContextOptionsBuilder<OrderingDbContext>()
    .UseSqlite($"Data Source={databasePath}")
    .EnableDetailedErrors()
    .Options;

await using var db = new OrderingDbContext(options);
await db.Database.MigrateAsync();
await DatabaseSeeder.SeedAsync(db);

var queries = new OrderQueries(db);
var query = queries.BuildOpenOrdersQuery(page: 0, pageSize: 20);
var orders = await query.ToListAsync();

Console.WriteLine($"Database: {databasePath}");
Console.WriteLine($"Open orders: {orders.Count}");
foreach (var order in orders)
{
    Console.WriteLine($"{order.Id} | {order.CustomerName} | {order.LineCount} lines | {order.Total:C}");
}

Console.WriteLine();
Console.WriteLine("Generated SQL:");
Console.WriteLine(query.ToQueryString());
