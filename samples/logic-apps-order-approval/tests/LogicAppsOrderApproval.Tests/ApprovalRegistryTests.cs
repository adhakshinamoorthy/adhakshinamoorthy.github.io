using LogicAppsOrderApproval;
using Xunit;

namespace LogicAppsOrderApproval.Tests;

public sealed class ApprovalRegistryTests
{
    [Fact]
    public void Record_is_idempotent_per_workflow_run()
    {
        var registry = new ApprovalRegistry();
        var callback = new ApprovalCallback("run-42", Guid.NewGuid(), "approved");
        var first = registry.Record(callback, DateTimeOffset.Parse("2026-08-15T00:00:00Z"));
        var replay = registry.Record(callback with { Decision = "rejected" }, DateTimeOffset.UtcNow);

        Assert.True(first.Created);
        Assert.False(replay.Created);
        Assert.Equal("approved", replay.Result.Decision);
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("")]
    public void Record_rejects_unknown_decisions(string decision)
    {
        var registry = new ApprovalRegistry();
        Assert.Throws<ArgumentException>(() => registry.Record(new ApprovalCallback("run-1", Guid.NewGuid(), decision), DateTimeOffset.UtcNow));
    }
}
