var target = 99.95;
var windowMinutes = 30d * 24 * 60;
var budgetMinutes = windowMinutes * (100 - target) / 100;
var incidents = new[] { new Incident("checkout", 7.5), new("checkout", 4.0) };
var consumed = incidents.Sum(x => x.Minutes);
Console.WriteLine($"SLO={target}% budget={budgetMinutes:F1}m consumed={consumed:F1}m remaining={budgetMinutes - consumed:F1}m");
if (args.Contains("--self-test") && (consumed >= budgetMinutes || budgetMinutes <= 0)) return 1;
return 0;
sealed record Incident(string Flow, double Minutes);
