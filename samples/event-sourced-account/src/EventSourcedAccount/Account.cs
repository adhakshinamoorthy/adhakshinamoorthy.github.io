namespace EventSourcedAccount;
public interface IAccountEvent;public sealed record AccountOpened(Guid Id,string Owner):IAccountEvent;public sealed record MoneyDeposited(decimal Amount):IAccountEvent;public sealed record MoneyWithdrawn(decimal Amount):IAccountEvent;
public sealed class Account
{private readonly List<IAccountEvent> changes=[];public Guid Id{get;private set;}public string Owner{get;private set;}="";public decimal Balance{get;private set;}public long Version{get;private set;}=-1;public IReadOnlyList<IAccountEvent> Changes=>changes;
 public static Account Open(Guid id,string owner){if(id==Guid.Empty||string.IsNullOrWhiteSpace(owner))throw new ArgumentException("ID and owner required.");var a=new Account();a.Record(new AccountOpened(id,owner.Trim()));return a;}
 public static Account Rehydrate(IEnumerable<IAccountEvent> history){var a=new Account();foreach(var e in history){a.Apply(e);a.Version++;}return a;}
 public void Deposit(decimal amount){if(amount<=0)throw new ArgumentOutOfRangeException(nameof(amount));Record(new MoneyDeposited(amount));}
 public void Withdraw(decimal amount){if(amount<=0||amount>Balance)throw new InvalidOperationException("Insufficient funds or invalid amount.");Record(new MoneyWithdrawn(amount));}
 public void MarkCommitted(){Version+=changes.Count;changes.Clear();}
 private void Record(IAccountEvent e){Apply(e);changes.Add(e);}private void Apply(IAccountEvent e){switch(e){case AccountOpened x:Id=x.Id;Owner=x.Owner;break;case MoneyDeposited x:Balance+=x.Amount;break;case MoneyWithdrawn x:Balance-=x.Amount;break;}}}
public sealed class EventStore
{private readonly Dictionary<Guid,List<IAccountEvent>> streams=[];public IReadOnlyList<IAccountEvent> Load(Guid id)=>streams.GetValueOrDefault(id)??[];public void Append(Guid id,long expectedVersion,IReadOnlyList<IAccountEvent> events){var stream=streams.GetValueOrDefault(id);var actual=(stream?.Count??0)-1;if(actual!=expectedVersion)throw new ConcurrencyException();stream??=[];stream.AddRange(events);streams[id]=stream;}}
public sealed class ConcurrencyException:Exception;
