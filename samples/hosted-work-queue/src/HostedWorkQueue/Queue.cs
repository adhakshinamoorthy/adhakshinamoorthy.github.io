using System.Threading.Channels;namespace HostedWorkQueue;public sealed record WorkItem(Guid Id,string Payload);
public sealed class WorkQueue
{private readonly Channel<WorkItem> channel;public WorkQueue(int capacity){channel=Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(capacity){FullMode=BoundedChannelFullMode.Wait,SingleReader=true});}public ValueTask EnqueueAsync(WorkItem item,CancellationToken ct=default)=>channel.Writer.WriteAsync(item,ct);public IAsyncEnumerable<WorkItem> ReadAllAsync(CancellationToken ct)=>channel.Reader.ReadAllAsync(ct);public void Complete()=>channel.Writer.TryComplete();}
public sealed class QueueWorker(WorkQueue queue,Func<WorkItem,CancellationToken,ValueTask> handler)
{public int Failures{get;private set;}public async Task RunAsync(CancellationToken ct){await foreach(var item in queue.ReadAllAsync(ct)){try{await handler(item,ct);}catch(OperationCanceledException)when(ct.IsCancellationRequested){throw;}catch{Failures++;}}}}
