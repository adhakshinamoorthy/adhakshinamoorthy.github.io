using TestcontainersPostgresInventory;

var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Set POSTGRES_CONNECTION_STRING to run the inventory workflow, or run the test suite to provision PostgreSQL automatically with Testcontainers.");
    return;
}

await using var repository = new InventoryRepository(connectionString);
await repository.EnsureSchemaAsync();
await repository.UpsertAsync(new InventoryItem("atlas-book", 7));
var item = await repository.FindAsync("atlas-book");
Console.WriteLine($"Inventory: {item?.Sku}={item?.Available}");
