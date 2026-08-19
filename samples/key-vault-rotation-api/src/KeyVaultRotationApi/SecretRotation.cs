using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace KeyVaultRotationApi;

public sealed record SecretVersion(string Name, string Version, string Value, DateTimeOffset CreatedAt);
public sealed record SecretLease(string Name, string Version, string Fingerprint, DateTimeOffset RefreshAfter);

public interface IVersionedSecretStore { SecretVersion GetActive(string name); }

public sealed class InMemoryVersionedSecretStore : IVersionedSecretStore
{
    private readonly ConcurrentDictionary<string, List<SecretVersion>> _versions = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _active = new(StringComparer.Ordinal);

    public SecretVersion Set(string name, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name); ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var item = new SecretVersion(name, Guid.NewGuid().ToString("N"), value, DateTimeOffset.UtcNow);
        var list = _versions.GetOrAdd(name, _ => []); lock (list) list.Add(item);
        _active.TryAdd(name, item.Version);
        return item with { Value = "[redacted]" };
    }
    public bool Activate(string name, string version) { if (!_versions.TryGetValue(name, out var list)) return false; lock (list) { if (list.All(item => item.Version != version)) return false; } _active[name] = version; return true; }
    public SecretVersion GetActive(string name) { if (!_versions.TryGetValue(name, out var list) || !_active.TryGetValue(name, out var version)) throw new KeyNotFoundException(name); lock (list) return list.Single(item => item.Version == version); }
}

public sealed class RotatingSecretCache(IVersionedSecretStore store, TimeProvider? clock = null, TimeSpan? lifetime = null)
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;
    private readonly TimeSpan _lifetime = lifetime ?? TimeSpan.FromMinutes(5);
    private readonly ConcurrentDictionary<string, (SecretVersion Secret, DateTimeOffset Expires)> _cache = new(StringComparer.Ordinal);

    public SecretLease Get(string name)
    {
        var now = _clock.GetUtcNow();
        var cached = _cache.AddOrUpdate(name, _ => Load(name, now), (_, current) => current.Expires > now ? current : Load(name, now));
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(cached.Secret.Value)))[..12];
        return new(name, cached.Secret.Version, fingerprint, cached.Expires);
    }
    public void Invalidate(string name) => _cache.TryRemove(name, out _);
    private (SecretVersion, DateTimeOffset) Load(string name, DateTimeOffset now) => (store.GetActive(name), now.Add(_lifetime));
}
