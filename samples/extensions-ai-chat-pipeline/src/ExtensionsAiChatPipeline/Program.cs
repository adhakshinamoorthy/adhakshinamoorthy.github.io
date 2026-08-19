using System.Diagnostics;

IChatClient client = new TimingClient(new CacheClient(new LocalChatClient()));
foreach (var prompt in new[] { "summarize order 42", "summarize order 42", "explain retry policy" })
    Console.WriteLine(await client.CompleteAsync(prompt));

if (args.Contains("--self-test") && CacheClient.Hits != 1) return 1;
return 0;

interface IChatClient { Task<string> CompleteAsync(string prompt); }
sealed class LocalChatClient : IChatClient
{
    public Task<string> CompleteAsync(string prompt) => Task.FromResult($"local:{prompt.ToUpperInvariant()}");
}
sealed class CacheClient(IChatClient inner) : IChatClient
{
    private readonly Dictionary<string, string> cache = new(StringComparer.Ordinal);
    public static int Hits { get; private set; }
    public async Task<string> CompleteAsync(string prompt)
    {
        if (cache.TryGetValue(prompt, out var value)) { Hits++; return $"cache:{value}"; }
        return cache[prompt] = await inner.CompleteAsync(prompt);
    }
}
sealed class TimingClient(IChatClient inner) : IChatClient
{
    public async Task<string> CompleteAsync(string prompt)
    {
        var timer = Stopwatch.StartNew();
        var value = await inner.CompleteAsync(prompt);
        return $"{value} ({timer.ElapsedMilliseconds} ms)";
    }
}
