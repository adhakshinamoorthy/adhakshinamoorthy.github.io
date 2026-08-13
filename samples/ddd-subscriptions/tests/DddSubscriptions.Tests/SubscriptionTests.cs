using DddSubscriptions;
using Xunit;

public sealed class SubscriptionTests
{
    [Fact] public void Start_establishes_valid_aggregate_and_event()
    {
        var customer = CustomerId.From(Guid.NewGuid());
        var subscription = Subscription.Start(customer, Plan.Create("starter", 19m));
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        var started = Assert.IsType<SubscriptionStarted>(Assert.Single(subscription.DomainEvents));
        Assert.Equal(customer, started.CustomerId);
    }

    [Fact] public void Plan_change_enforces_invariant_and_records_fact()
    {
        var subscription = Subscription.Start(CustomerId.From(Guid.NewGuid()), Plan.Create("starter", 19m));
        subscription.DequeueEvents(); subscription.ChangePlan(Plan.Create("growth", 49m));
        var changed = Assert.IsType<SubscriptionPlanChanged>(Assert.Single(subscription.DomainEvents));
        Assert.Equal("STARTER", changed.PreviousPlan); Assert.Equal("GROWTH", changed.NewPlan);
        Assert.Throws<InvalidOperationException>(() => subscription.ChangePlan(Plan.Create("growth", 49m)));
    }

    [Fact] public void Cancelled_aggregate_rejects_future_changes()
    {
        var subscription = Subscription.Start(CustomerId.From(Guid.NewGuid()), Plan.Create("starter", 19m));
        subscription.Cancel("No longer needed");
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
        Assert.Throws<InvalidOperationException>(() => subscription.ChangePlan(Plan.Create("growth", 49m)));
    }

    [Fact] public void Value_objects_reject_invalid_values()
    {
        Assert.Throws<ArgumentException>(() => CustomerId.From(Guid.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(() => Plan.Create("free", 0m));
    }
}
