var services = new[]
{
    new Service("orders", "commerce", 1, "https://github.com/example/orders", true, true, true),
    new Service("catalog", "commerce", 2, "https://github.com/example/catalog", true, true, true)
};
var errors = services.SelectMany(Validate).ToArray();
foreach (var service in services) Console.WriteLine($"{service.Name}: owner={service.Owner} tier={service.Tier}");
if (args.Contains("--self-test") && errors.Length != 0) return 1;
return errors.Length == 0 ? 0 : 2;
static IEnumerable<string> Validate(Service s)
{
    if (string.IsNullOrWhiteSpace(s.Owner)) yield return $"{s.Name}: owner missing";
    if (s.Tier is < 1 or > 4) yield return $"{s.Name}: invalid tier";
    if (!s.Health || !s.Telemetry || !s.Runbook) yield return $"{s.Name}: operational metadata incomplete";
}
sealed record Service(string Name, string Owner, int Tier, string Repository, bool Health, bool Telemetry, bool Runbook);
