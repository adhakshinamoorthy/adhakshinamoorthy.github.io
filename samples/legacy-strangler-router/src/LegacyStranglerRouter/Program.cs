var router = new StranglerRouter(30);
var requests = Enumerable.Range(1, 20).Select(i => $"customer-{i}").ToArray();
var routes = requests.GroupBy(router.Route).ToDictionary(x => x.Key, x => x.Count());
Console.WriteLine($"legacy={routes.GetValueOrDefault("legacy")} modern={routes.GetValueOrDefault("modern")}");
router.Rollback();
if (args.Contains("--self-test") && requests.Any(x => router.Route(x) != "legacy")) return 1;
return 0;
sealed class StranglerRouter(int percentage)
{
    private int percentage = Math.Clamp(percentage, 0, 100);
    public string Route(string key) => Math.Abs(StringComparer.Ordinal.GetHashCode(key)) % 100 < percentage ? "modern" : "legacy";
    public void Rollback() => percentage = 0;
}
