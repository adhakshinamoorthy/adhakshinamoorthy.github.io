var gates = new[]
{
    new Gate("Owner and on-call", true), new("SLO dashboard", true), new("Actionable alerts", true),
    new("Rollback rehearsed", true), new("Restore evidence", false), new("Capacity evidence", true)
};
foreach (var gate in gates) Console.WriteLine($"{(gate.Ready ? "PASS" : "BLOCK")} {gate.Name}");
var ready = gates.All(x => x.Ready);
if (args.Contains("--self-test") && ready) return 1;
return 0;
sealed record Gate(string Name, bool Ready);
