using Xunit;

namespace SecretsRotation.Tests;

public sealed class SecretProviderTests
{
    [Fact]
    public async Task Cached_value_is_reused_before_expiry()
    {
        var time = new ManualTimeProvider();
        var vault = new InMemorySecretVault(time);
        var first = vault.Rotate("api-key", "one");
        var provider = new CachedSecretProvider(vault, time, TimeSpan.FromMinutes(5));
        Assert.Equal(first, await provider.GetAsync("api-key", default));
        vault.Rotate("api-key", "two");
        Assert.Equal(first, await provider.GetAsync("api-key", default));
    }

    [Fact]
    public async Task Rotated_value_is_loaded_after_expiry()
    {
        var time = new ManualTimeProvider();
        var vault = new InMemorySecretVault(time);
        vault.Rotate("api-key", "one");
        var provider = new CachedSecretProvider(vault, time, TimeSpan.FromMinutes(5));
        var first = await provider.GetAsync("api-key", default);
        var second = vault.Rotate("api-key", "two");
        time.Advance(TimeSpan.FromMinutes(6));
        Assert.NotEqual(first.Version, (await provider.GetAsync("api-key", default)).Version);
        Assert.Equal(second.Version, (await provider.GetAsync("api-key", default)).Version);
    }

    [Fact]
    public void Diagnostics_redact_secret_value()
    {
        var secret = new SecretValue("never-print-this", "0001", DateTimeOffset.UnixEpoch);
        Assert.DoesNotContain("never-print-this", secret.ToString(), StringComparison.Ordinal);
        Assert.Contains("[REDACTED]", secret.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Missing_secret_fails_without_inventing_a_default()
    {
        var time = new ManualTimeProvider();
        var provider = new CachedSecretProvider(new InMemorySecretVault(time), time, TimeSpan.FromMinutes(1));
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await provider.GetAsync("missing", default));
    }
}

internal sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow = DateTimeOffset.UnixEpoch;
    public override DateTimeOffset GetUtcNow() => _utcNow;
    public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);
}
