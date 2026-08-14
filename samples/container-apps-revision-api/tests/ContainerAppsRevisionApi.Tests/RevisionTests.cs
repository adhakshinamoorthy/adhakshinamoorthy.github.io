using ContainerAppsRevisionApi;
using Xunit;

namespace ContainerAppsRevisionApi.Tests;

public sealed class RevisionTests
{
    [Fact]
    public void Readiness_changes_atomically()
    {
        var state = new ReadinessState();
        Assert.False(state.IsReady);
        state.MarkReady();
        Assert.True(state.IsReady);
        state.MarkUnready();
        Assert.False(state.IsReady);
    }

    [Fact]
    public void Revision_has_safe_local_defaults()
    {
        var revision = RevisionInfo.FromEnvironment();
        Assert.False(string.IsNullOrWhiteSpace(revision.Name));
        Assert.False(string.IsNullOrWhiteSpace(revision.Replica));
        Assert.False(string.IsNullOrWhiteSpace(revision.Region));
    }
}
