var costs = new[] { new Cost("Compute", 4200, .8), new("Database", 3100, .95), new("Observability", 900, .7), new("Idle dev", 650, 0) };
var orders = 125_000;
var total = costs.Sum(x => x.Amount);
var unit = total / orders;
var waste = costs.Where(x => x.Utilization < .1).Sum(x => x.Amount);
Console.WriteLine($"total={total:C0} cost/order={unit:C4} idle={waste:C0}");
if (args.Contains("--self-test") && (unit <= 0 || waste != 650)) return 1;
return 0;
sealed record Cost(string Meter, decimal Amount, double Utilization);
