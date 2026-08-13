namespace HealthReadinessLab;

public enum HealthStatus { Healthy, Degraded, Unhealthy }
public sealed record HealthResult(string Name, HealthStatus Status, string Description);
public interface IHealthProbe { string Name { get; } ValueTask<HealthResult> CheckAsync(CancellationToken ct); }
public sealed class HealthEvaluator(IEnumerable<IHealthProbe> probes)
{ public async ValueTask<IReadOnlyList<HealthResult>> CheckAsync(CancellationToken ct = default) { var results = new List<HealthResult>(); foreach (var p in probes) results.Add(await p.CheckAsync(ct)); return results; } public static HealthStatus Aggregate(IEnumerable<HealthResult> r) => r.Select(x => x.Status).DefaultIfEmpty(HealthStatus.Healthy).Max(); }
public sealed class StartupState { public bool Ready { get; set; } }
public sealed class StartupProbe(StartupState state) : IHealthProbe { public string Name => "startup"; public ValueTask<HealthResult> CheckAsync(CancellationToken ct) => ValueTask.FromResult(new HealthResult(Name, state.Ready ? HealthStatus.Healthy : HealthStatus.Unhealthy, state.Ready ? "initialized" : "starting")); }
public sealed class DependencyProbe(Func<CancellationToken, ValueTask<bool>> check) : IHealthProbe { public string Name => "database"; public async ValueTask<HealthResult> CheckAsync(CancellationToken ct) => new(Name, await check(ct) ? HealthStatus.Healthy : HealthStatus.Unhealthy, "bounded dependency check"); }
