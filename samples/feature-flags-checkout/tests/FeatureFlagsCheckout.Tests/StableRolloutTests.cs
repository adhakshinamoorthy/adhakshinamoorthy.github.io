using Xunit;

namespace FeatureFlagsCheckout.Tests;

public sealed class StableRolloutTests
{
    [Fact] public void Same_user_gets_stable_bucket() => Assert.Equal(StableRolloutEvaluator.Bucket("flag", "user-1"), StableRolloutEvaluator.Bucket("flag", "user-1"));
    [Fact] public void Targeted_group_bypasses_percentage() { var evaluator = new StableRolloutEvaluator(new("flag", true, 0, ["staff"], "team", new(2027, 1, 1))); Assert.True(evaluator.Evaluate("user", ["staff"]).Enabled); }
    [Fact] public void Global_disable_is_safe_fallback() { var evaluator = new StableRolloutEvaluator(new("flag", false, 100, ["staff"], "team", new(2027, 1, 1))); Assert.False(evaluator.Evaluate("user", ["staff"]).Enabled); }
}
