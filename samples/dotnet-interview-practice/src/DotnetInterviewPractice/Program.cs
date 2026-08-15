var prompts = new[]
{
    new Prompt("Design a reliable checkout API", new[] { "context", "trade-off", "failure", "evidence" }),
    new("Explain idempotency", new[] { "definition", "example", "failure", "verification" })
};
var answer = "context trade-off failure evidence rollback measurement";
foreach (var prompt in prompts)
{
    var score = prompt.Criteria.Count(c => answer.Contains(c, StringComparison.OrdinalIgnoreCase));
    Console.WriteLine($"{score}/{prompt.Criteria.Length} {prompt.Question}");
}
if (args.Contains("--self-test") && prompts[0].Criteria.Any(c => !answer.Contains(c))) return 1;
return 0;
sealed record Prompt(string Question, string[] Criteria);
