using Xunit;

namespace AdfIncrementalOrders.Tests;

public sealed class IncrementalPipelineTests
{
    [Fact] public void Run_commits_only_new_changes() { var pipeline = new IncrementalOrderPipeline(); pipeline.AddSourceChange(new("o-1", 10, 1)); Assert.Equal(1, pipeline.Run().Written); Assert.Equal(0, pipeline.Run().Written); }
    [Fact] public void Replay_is_idempotent_and_newer_change_wins() { var pipeline = new IncrementalOrderPipeline(); pipeline.AddSourceChange(new("o-1", 10, 1)); pipeline.Run(); pipeline.AddSourceChange(new("o-1", 12, 2)); Assert.Equal(1, pipeline.Run().Written); Assert.Equal(12, pipeline.Status().CuratedOrders.Single().Total); }
    [Fact] public void Failed_validation_does_not_advance_watermark() { var pipeline = new IncrementalOrderPipeline(); pipeline.AddSourceChange(new("o-1", 10, 1)); var failed = pipeline.Run(_ => false); Assert.False(failed.WatermarkCommitted); Assert.Equal(1, pipeline.Run().Written); }
}
