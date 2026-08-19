var controls = new[]
{
    new Control("Identity", true, 10), new("Resource organization", true, 8), new("Network", true, 9),
    new("Policy", true, 10), new("Operations", true, 9), new("Cost", false, 7), new("Recovery", true, 10)
};
var score = controls.Where(x => x.Ready).Sum(x => x.Weight) * 100d / controls.Sum(x => x.Weight);
foreach (var gap in controls.Where(x => !x.Ready)) Console.WriteLine($"gap={gap.Area} weight={gap.Weight}");
Console.WriteLine($"readiness={score:F0}%");
if (args.Contains("--self-test") && score is <= 0 or >= 100) return 1;
return 0;
sealed record Control(string Area, bool Ready, int Weight);
