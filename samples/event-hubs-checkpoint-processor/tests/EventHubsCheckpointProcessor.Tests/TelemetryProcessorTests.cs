using EventHubsCheckpointProcessor;
using Xunit;

namespace EventHubsCheckpointProcessor.Tests;

public sealed class TelemetryProcessorTests
{
    [Fact]
    public void Same_partition_key_is_stable()
    {
        var first = LocalEventHub.SelectPartition("device-42", 8);
        Assert.Equal(first, LocalEventHub.SelectPartition("device-42", 8));
    }

    [Fact]
    public async Task Processor_checkpoints_after_processing()
    {
        var hub = new LocalEventHub();
        var checkpoints = new CheckpointStore();
        var sink = new IdempotentTelemetrySink();
        var item = hub.Publish(new TelemetryInput("event-1", "device-42", 21.5), DateTimeOffset.UtcNow);
        var processor = new TelemetryProcessor(hub, checkpoints, sink);

        Assert.Equal(1, await processor.ProcessAsync(item.PartitionId, 100, CancellationToken.None));
        Assert.Equal(item.SequenceNumber, checkpoints.Get(item.PartitionId));
        Assert.Equal(1, sink.Count);
    }

    [Fact]
    public async Task Idempotent_sink_absorbs_replay_after_checkpoint_loss()
    {
        var hub = new LocalEventHub();
        var sink = new IdempotentTelemetrySink();
        var item = hub.Publish(new TelemetryInput("event-1", "device-42", 21.5), DateTimeOffset.UtcNow);

        await new TelemetryProcessor(hub, new CheckpointStore(), sink).ProcessAsync(item.PartitionId, 100, CancellationToken.None);
        await new TelemetryProcessor(hub, new CheckpointStore(), sink).ProcessAsync(item.PartitionId, 100, CancellationToken.None);

        Assert.Equal(1, sink.Count);
    }
}
