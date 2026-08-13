using DddSubscriptions;

var subscription = Subscription.Start(CustomerId.From(Guid.NewGuid()), Plan.Create("starter", 19m));
subscription.ChangePlan(Plan.Create("growth", 49m));
Console.WriteLine($"{subscription.Id}: {subscription.CurrentPlan.Code} [{subscription.Status}], events={subscription.DomainEvents.Count}");
