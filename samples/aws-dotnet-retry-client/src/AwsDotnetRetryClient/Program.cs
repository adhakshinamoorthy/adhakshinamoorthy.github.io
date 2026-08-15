var failures = new[] { new CloudFailure("Throttling", true), new("TimeoutBeforeSend", true), new("Validation", false) };
var random = new Random(42);
foreach (var failure in failures)
{
    var delays = Enumerable.Range(1, 3).Where(_ => failure.Retryable).Select(attempt => Math.Min(1000, 100 * (1 << attempt)) + random.Next(0, 50));
    Console.WriteLine($"{failure.Code}: [{string.Join(',', delays)}]");
}
if (args.Contains("--self-test") && failures.Single(x => x.Code == "Validation").Retryable) return 1;
return 0;
sealed record CloudFailure(string Code, bool Retryable);
