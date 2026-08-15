using System.Collections.Concurrent;

namespace AdfIncrementalOrders;

public sealed record OrderChange(string OrderId, decimal Total, long Watermark);
public sealed record CuratedOrder(string OrderId, decimal Total, long SourceWatermark);
public sealed record PipelineRun(long FromExclusive, long ToInclusive, int Read, int Written, bool WatermarkCommitted);
public sealed record PipelineStatus(long CommittedWatermark, IReadOnlyList<CuratedOrder> CuratedOrders);

public sealed class IncrementalOrderPipeline
{
    private readonly List<OrderChange> _source = [];
    private readonly ConcurrentDictionary<string, CuratedOrder> _sink = new(StringComparer.Ordinal);
    private long _committedWatermark;

    public OrderChange AddSourceChange(OrderChange change)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(change.OrderId);
        if (change.Total < 0) throw new ArgumentException("Total cannot be negative.");
        lock (_source)
        {
            if (_source.Any(item => item.Watermark == change.Watermark)) throw new ArgumentException("Watermark must be unique.");
            _source.Add(change);
        }
        return change;
    }

    public PipelineRun Run(Func<OrderChange, bool>? validator = null)
    {
        OrderChange[] batch;
        long from;
        lock (_source)
        {
            from = _committedWatermark;
            batch = _source.Where(item => item.Watermark > from).OrderBy(item => item.Watermark).ToArray();
        }
        if (batch.Length == 0) return new(from, from, 0, 0, true);
        if (validator is not null && batch.Any(item => !validator(item))) return new(from, batch[^1].Watermark, batch.Length, 0, false);

        foreach (var item in batch)
            _sink.AddOrUpdate(item.OrderId, new CuratedOrder(item.OrderId, item.Total, item.Watermark), (_, current) =>
                item.Watermark >= current.SourceWatermark ? new CuratedOrder(item.OrderId, item.Total, item.Watermark) : current);

        Interlocked.Exchange(ref _committedWatermark, batch[^1].Watermark);
        return new(from, batch[^1].Watermark, batch.Length, batch.Length, true);
    }

    public PipelineStatus Status() => new(Interlocked.Read(ref _committedWatermark), _sink.Values.OrderBy(item => item.OrderId).ToArray());
}
