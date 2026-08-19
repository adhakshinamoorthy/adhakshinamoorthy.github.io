var findings = new[]
{
    new Finding("Reliability", "Checkout", 5, 4, false), new("Security", "Administration", 5, 3, true),
    new("Cost", "Reporting", 2, 3, false), new("Operations", "All", 4, 4, false), new("Performance", "Search", 3, 4, true)
};
foreach (var item in findings.OrderByDescending(x => x.Impact * x.Likelihood))
    Console.WriteLine($"{item.Pillar}/{item.Flow}: risk={item.Impact * item.Likelihood} evidence={item.HasEvidence}");
if (args.Contains("--self-test") && findings.Select(x => x.Pillar).Distinct().Count() != 5) return 1;
return 0;
sealed record Finding(string Pillar, string Flow, int Impact, int Likelihood, bool HasEvidence);
