using KubernetesInventoryApi;
using Xunit;

namespace KubernetesInventoryApi.Tests;

public sealed class ReadinessStateTests
{
    [Fact]
    public void State_is_not_ready_until_startup_completes()
    {
        var state = new ReadinessState();
        Assert.False(state.IsReady);
        state.MarkReady();
        Assert.True(state.IsReady);
    }

    [Fact]
    public void Shutdown_removes_readiness()
    {
        var state = new ReadinessState();
        state.MarkReady();
        state.MarkNotReady();
        Assert.False(state.IsReady);
    }

    [Fact]
    public void Deployment_declares_safe_rollout_and_probe_contracts()
    {
        var manifest = File.ReadAllText(FindRepositoryFile("k8s", "deployment.yaml"));
        Assert.Contains("maxUnavailable: 0", manifest, StringComparison.Ordinal);
        Assert.Contains("readinessProbe:", manifest, StringComparison.Ordinal);
        Assert.Contains("livenessProbe:", manifest, StringComparison.Ordinal);
        Assert.Contains("startupProbe:", manifest, StringComparison.Ordinal);
        Assert.Contains("runAsNonRoot: true", manifest, StringComparison.Ordinal);
        Assert.Contains("requests:", manifest, StringComparison.Ordinal);
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }

        throw new FileNotFoundException(string.Join('/', segments));
    }
}
