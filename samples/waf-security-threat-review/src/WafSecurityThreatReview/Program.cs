var threats = new[]
{
    new Threat("Spoof workload identity", "Order data", true, true, 4),
    new("Exfiltrate logs", "Customer data", true, true, 3),
    new("Abuse admin action", "Control plane", true, false, 5)
};
foreach (var threat in threats.OrderByDescending(x => x.ResidualRisk))
    Console.WriteLine($"risk={threat.ResidualRisk} {threat.Name} prevent={threat.Prevent} detect={threat.Detect}");
var criticalUndetected = threats.Any(x => x.ResidualRisk >= 5 && !x.Detect);
if (args.Contains("--self-test") && !criticalUndetected) return 1;
return 0;
sealed record Threat(string Name, string Asset, bool Prevent, bool Detect, int ResidualRisk);
