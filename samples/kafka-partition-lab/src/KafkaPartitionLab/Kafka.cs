namespace KafkaPartitionLab;
public sealed record Record(int Partition,long Offset,string Key,string Value);
public sealed class TopicLog
{private readonly List<Record>[] partitions;public TopicLog(int count){if(count<1)throw new ArgumentOutOfRangeException(nameof(count));partitions=Enumerable.Range(0,count).Select(_=>new List<Record>()).ToArray();}public int PartitionCount=>partitions.Length;public Record Produce(string key,string value){var partition=(int)((uint)StringComparer.Ordinal.GetHashCode(key)%(uint)partitions.Length);var r=new Record(partition,partitions[partition].Count,key,value);partitions[partition].Add(r);return r;}public IReadOnlyList<Record> Read(int partition,long offset,int max)=>partitions[partition].Where(x=>x.Offset>=offset).Take(max).ToArray();}
public sealed class ConsumerGroup(TopicLog topic)
{private readonly Dictionary<int,long> committed=[];public IReadOnlyList<Record> Poll(int partition,int max=10)=>topic.Read(partition,committed.GetValueOrDefault(partition),max);public void Commit(Record record)=>committed[record.Partition]=record.Offset+1;public long Position(int partition)=>committed.GetValueOrDefault(partition);}
