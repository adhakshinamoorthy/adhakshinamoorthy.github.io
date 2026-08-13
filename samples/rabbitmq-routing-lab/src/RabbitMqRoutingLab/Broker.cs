namespace RabbitMqRoutingLab;
public sealed record Message(Guid Id,string RoutingKey,string Body);
public sealed class QueueState(string name){private readonly Queue<Message> ready=new();private readonly Dictionary<Guid,Message> unacked=[];public string Name{get;}=name;public int Ready=>ready.Count;public int Unacked=>unacked.Count;public void Enqueue(Message m)=>ready.Enqueue(m);public Message? Deliver(){if(!ready.TryDequeue(out var m))return null;unacked.Add(m.Id,m);return m;}public void Ack(Guid id)=>unacked.Remove(id);public void Nack(Guid id,bool requeue){if(!unacked.Remove(id,out var m))return;if(requeue)ready.Enqueue(m);}}
public sealed class TopicExchange
{
 private readonly List<(string Pattern,QueueState Queue)> bindings=[];
 public void Bind(string pattern,QueueState queue)=>bindings.Add((pattern,queue));
 public int Publish(Message message){var matched=0;foreach(var b in bindings.Where(x=>Matches(x.Pattern,message.RoutingKey))){b.Queue.Enqueue(message);matched++;}return matched;}
 public static bool Matches(string pattern,string key){var p=pattern.Split('.');var k=key.Split('.');return Match(p,0,k,0);}
 private static bool Match(string[] p,int pi,string[] k,int ki){if(pi==p.Length)return ki==k.Length;if(p[pi]=="#")return pi==p.Length-1||Enumerable.Range(ki,k.Length-ki+1).Any(next=>Match(p,pi+1,k,next));if(ki==k.Length)return false;return(p[pi]=="*"||p[pi]==k[ki])&&Match(p,pi+1,k,ki+1);}
}
