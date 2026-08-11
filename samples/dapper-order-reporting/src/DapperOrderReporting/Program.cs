using DapperOrderReporting.Models;
using DapperOrderReporting.Persistence;

var databaseArgument = args
    .Select((value, index) => (value, index))
    .FirstOrDefault(item => item.value == "--database");
var databasePath = databaseArgument.value is not null && databaseArgument.index + 1 < args.Length
    ? args[databaseArgument.index + 1]
    : Path.Combine("artifacts", "dapper-orders.db");
databasePath = Path.GetFullPath(databasePath);
Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

var connections = new SqliteConnectionFactory($"Data Source={databasePath};Pooling=False");
await new DatabaseInitializer(connections).InitializeAsync();

var queries = new OrderQueries(connections);
var summaries = await queries.SearchAsync(new OrderSearch(
    Status: "Placed",
    MinimumTotalCents: 5000,
    Sort: OrderSort.HighestValue));
var dashboard = await queries.GetDashboardAsync();

Console.WriteLine($"Database: {databasePath}");
Console.WriteLine("Placed orders worth at least 50.00:");
foreach (var order in summaries)
{
    Console.WriteLine($"{order.Id} | {order.CustomerName} | {order.LineCount} lines | {order.TotalCents / 100m:C}");
}

Console.WriteLine();
Console.WriteLine("Orders by status:");
foreach (var status in dashboard.Statuses)
{
    Console.WriteLine($"{status.Status}: {status.OrderCount}");
}
