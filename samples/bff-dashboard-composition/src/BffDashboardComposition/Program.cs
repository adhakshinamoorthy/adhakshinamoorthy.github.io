var composer = new DashboardComposer();
var view = await composer.LoadAsync("user-17", TimeSpan.FromSeconds(1));
Console.WriteLine($"orders={view.OrderCount} stock={view.LowStockCount} warnings={string.Join(',', view.Warnings)}");
if (args.Contains("--self-test") && view.OrderCount != 3) return 1;
return 0;

sealed record Dashboard(int OrderCount, int LowStockCount, string[] Warnings);
sealed class DashboardComposer
{
    public async Task<Dashboard> LoadAsync(string userId, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var orders = GetOrders(userId, cts.Token);
        var stock = GetStock(cts.Token);
        await Task.WhenAll(orders, stock);
        return new(await orders, await stock, []);
    }
    static async Task<int> GetOrders(string userId, CancellationToken ct) { await Task.Delay(10, ct); return 3; }
    static async Task<int> GetStock(CancellationToken ct) { await Task.Delay(10, ct); return 2; }
}
