using System.Collections.Concurrent;

namespace SecretsRotation;

public sealed record SecretValue(string Value, string Version, DateTimeOffset CreatedAtUtc)
{
    public override string ToString() => $"SecretValue {{ Version = {Version}, CreatedAtUtc = {CreatedAtUtc:O}, Value = [REDACTED] }}";
}

public interface ISecretVault
{
    ValueTask<SecretValue> GetCurrentAsync(string name, CancellationToken cancellationToken);
}

public sealed class InMemorySecretVault(TimeProvider timeProvider) : ISecretVault
{
    private readonly ConcurrentDictionary<string, SecretValue> _current = new(StringComparer.OrdinalIgnoreCase);
    private long _version;

    public SecretValue Rotate(string name, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var secret = new SecretValue(value, Interlocked.Increment(ref _version).ToString("D4"), timeProvider.GetUtcNow());
        _current[name] = secret;
        return secret;
    }

    public ValueTask<SecretValue> GetCurrentAsync(string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _current.TryGetValue(name, out var secret)
            ? ValueTask.FromResult(secret)
            : ValueTask.FromException<SecretValue>(new KeyNotFoundException($"Secret '{name}' was not found."));
    }
}

public sealed class CachedSecretProvider(ISecretVault vault, TimeProvider timeProvider, TimeSpan cacheDuration)
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new(StringComparer.OrdinalIgnoreCase);

    public async ValueTask<SecretValue> GetAsync(string name, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        if (_cache.TryGetValue(name, out var cached) && cached.ExpiresAtUtc > now) return cached.Secret;
        var fresh = await vault.GetCurrentAsync(name, cancellationToken);
        _cache[name] = new CacheEntry(fresh, now.Add(cacheDuration));
        return fresh;
    }

    private sealed record CacheEntry(SecretValue Secret, DateTimeOffset ExpiresAtUtc);
}
