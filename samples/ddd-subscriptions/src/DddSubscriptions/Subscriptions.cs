namespace DddSubscriptions;

public readonly record struct CustomerId
{
    private CustomerId(Guid value) => Value = value;
    public Guid Value { get; }
    public static CustomerId From(Guid value) => value == Guid.Empty ? throw new ArgumentException("Customer ID cannot be empty.", nameof(value)) : new(value);
}

public sealed record Plan
{
    private Plan(string code, decimal monthlyPrice) => (Code, MonthlyPrice) = (code, monthlyPrice);
    public string Code { get; }
    public decimal MonthlyPrice { get; }
    public static Plan Create(string code, decimal monthlyPrice)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Plan code is required.", nameof(code));
        if (monthlyPrice <= 0) throw new ArgumentOutOfRangeException(nameof(monthlyPrice));
        return new(code.Trim().ToUpperInvariant(), monthlyPrice);
    }
}

public interface IDomainEvent;
public sealed record SubscriptionStarted(Guid SubscriptionId, CustomerId CustomerId, string PlanCode) : IDomainEvent;
public sealed record SubscriptionPlanChanged(Guid SubscriptionId, string PreviousPlan, string NewPlan) : IDomainEvent;
public sealed record SubscriptionCancelled(Guid SubscriptionId, string Reason) : IDomainEvent;

public sealed class Subscription
{
    private readonly List<IDomainEvent> events = [];
    private Subscription(Guid id, CustomerId customerId, Plan plan) => (Id, CustomerId, CurrentPlan, Status) = (id, customerId, plan, SubscriptionStatus.Active);
    public Guid Id { get; }
    public CustomerId CustomerId { get; }
    public Plan CurrentPlan { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public IReadOnlyList<IDomainEvent> DomainEvents => events;

    public static Subscription Start(CustomerId customerId, Plan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var subscription = new Subscription(Guid.NewGuid(), customerId, plan);
        subscription.events.Add(new SubscriptionStarted(subscription.Id, customerId, plan.Code));
        return subscription;
    }

    public void ChangePlan(Plan nextPlan)
    {
        ArgumentNullException.ThrowIfNull(nextPlan);
        EnsureActive();
        if (nextPlan.Code == CurrentPlan.Code) throw new InvalidOperationException("Subscription already uses this plan.");
        var previous = CurrentPlan;
        CurrentPlan = nextPlan;
        events.Add(new SubscriptionPlanChanged(Id, previous.Code, nextPlan.Code));
    }

    public void Cancel(string reason)
    {
        EnsureActive();
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 200) throw new ArgumentException("Cancellation reason must contain 1 to 200 characters.", nameof(reason));
        Status = SubscriptionStatus.Cancelled;
        events.Add(new SubscriptionCancelled(Id, reason.Trim()));
    }

    public IReadOnlyList<IDomainEvent> DequeueEvents() { var pending = events.ToArray(); events.Clear(); return pending; }
    private void EnsureActive() { if (Status != SubscriptionStatus.Active) throw new InvalidOperationException("Only active subscriptions can change."); }
}

public enum SubscriptionStatus { Active, Cancelled }
