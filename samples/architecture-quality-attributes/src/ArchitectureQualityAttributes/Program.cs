var scenarios = new[]
{
    new Scenario("Reliability", "region unavailable", "peak", "checkout", "recover", 15, 9),
    new Scenario("Performance", "500 requests/s", "normal", "catalog", "respond", 250, 7),
    new Scenario("Security", "stolen credential", "any", "admin API", "deny and alert", 5, 10)
};
foreach (var item in scenarios.OrderByDescending(x => x.Risk * (1000d / x.Target)))
    Console.WriteLine($"{item.Attribute}: {item.Response} within {item.Target} ms/min; risk={item.Risk}");
if (args.Contains("--self-test") && scenarios.Any(x => x.Target <= 0 || x.Risk is < 1 or > 10)) return 1;
return 0;
sealed record Scenario(string Attribute, string Stimulus, string Environment, string Artifact, string Response, int Target, int Risk);
