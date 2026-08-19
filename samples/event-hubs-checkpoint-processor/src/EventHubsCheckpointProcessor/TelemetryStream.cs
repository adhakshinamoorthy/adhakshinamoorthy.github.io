using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace EventHubsCheckpointProcessor;

public sealed record TelemetryInput(string EventId, string DeviceId, double Value);
public sealed record StreamEvent(string EventId, string DeviceId, double Value, string PartitionId, long SequenceNumber, DateTimeOffset EnqueuedAt);

public sealed class LocalEventHub(int partitionCount = 4)
{
    private readonly List<StreamEvent>[] _partitions = Enumerable.Range(0, partitionCount).Select(_ => new List<StreamEvent>()).ToArray();

    public StreamEvent Publish(TelemetryInput input, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input.EventId);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.DeviceId);
        var partition = SelectPartition(input.DeviceId, _partitions.Length);
        lock (_partitions[partition])
        {
            var item = new StreamEvent(input.EventId, input.DeviceId, input.Value, partition.ToString(), _partitions[partition].Count, now);
            _partitions[partition].Add(item);
            return item;
        }
    }

    public IReadOnlyList<StreamEvent> ReadAfter(string partitionId, long checkpoint, int maximum)
    {
        var partition = int.Parse(partitionId, System.Globalization.CultureInfo.InvariantCulture);
        lock (_partitions[partition])
        {
            return _partitions[partition].Where(item => item.SequenceNumber > checkpoint).Take(maximum).ToArray();
        }
    }

    public static int SelectPartition(string key, int partitionCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (partitionCount <= 0) throw new ArgumentOutOfRangeException(nameof(partitionCount));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return (int)(BitConverter.ToUInt32(hash, 0) % (uint)partitionCount);
    }
}

public sealed class CheckpointStore
{
    private readonly ConcurrentDictionary<string, long> _checkpoints = new();
    public long Get(string partitionId) => _checkpoints.GetValueOrDefault(partitionId, -1);
    public void Save(string partitionId, long sequenceNumber) => _checkpoints.AddOrUpdate(partitionId, sequenceNumber, (_, current) => Math.Max(current, sequenceNumber));
    public IReadOnlyDictionary<string, long> Snapshot() => new Dictionary<string, long>(_checkpoints);
}

public sealed class IdempotentTelemetrySink
{
    private readonly ConcurrentDictionary<string, StreamEvent> _events = new(StringComparer.Ordinal);
    public bool Write(StreamEvent item) => _events.TryAdd(item.EventId, item);
    public int Count => _events.Count;
}

public sealed class TelemetryProcessor(LocalEventHub hub, CheckpointStore checkpoints, IdempotentTelemetrySink sink)
{
    public Task<int> ProcessAsync(string partitionId, int maximum, CancellationToken cancellationToken)
    {
        var processed = 0;
        foreach (var item in hub.ReadAfter(partitionId, checkpoints.Get(partitionId), maximum))
        {
            cancellationToken.ThrowIfCancellationRequested();
            sink.Write(item);
            checkpoints.Save(partitionId, item.SequenceNumber);
            processed++;
        }

        return Task.FromResult(processed);
    }
}
