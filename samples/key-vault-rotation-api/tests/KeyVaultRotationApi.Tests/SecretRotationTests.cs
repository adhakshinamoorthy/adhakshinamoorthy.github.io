using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace KeyVaultRotationApi.Tests;

public sealed class SecretRotationTests
{
    [Fact] public void Secret_value_is_not_returned_by_lease() { var store = new InMemoryVersionedSecretStore(); store.Set("Payments--ApiKey", "sensitive"); var lease = new RotatingSecretCache(store).Get("Payments--ApiKey"); Assert.DoesNotContain("sensitive", lease.ToString()); }
    [Fact] public void New_version_is_not_used_before_activation() { var store = new InMemoryVersionedSecretStore(); var first = store.Set("key", "one"); store.Set("key", "two"); Assert.Equal(first.Version, new RotatingSecretCache(store).Get("key").Version); }
    [Fact] public void Invalidating_cache_adopts_activated_version() { var store = new InMemoryVersionedSecretStore(); store.Set("key", "one"); var next = store.Set("key", "two"); var cache = new RotatingSecretCache(store); store.Activate("key", next.Version); cache.Invalidate("key"); Assert.Equal(next.Version, cache.Get("key").Version); }
    [Fact] public void Api_services_resolve_cache_from_store_interface() { var services = new ServiceCollection().AddSingleton<InMemoryVersionedSecretStore>().AddSingleton<IVersionedSecretStore>(provider => provider.GetRequiredService<InMemoryVersionedSecretStore>()).AddSingleton<RotatingSecretCache>().BuildServiceProvider(); Assert.NotNull(services.GetRequiredService<RotatingSecretCache>()); }
}
