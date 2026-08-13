using EventDrivenOrders; using Xunit;
public sealed class MessagingTests
{
 [Fact] public void Business_change_and_outbox_are_recorded_together(){var db=new OrderDatabase();var id=db.Place("c1",10m);Assert.Equal(10m,db.Orders[id]);Assert.Single(db.Outbox);}
 [Fact] public async Task Relay_publishes_pending_and_marks_sent(){var db=new OrderDatabase();var consumer=new LoyaltyConsumer();db.Place("c1",10m);Assert.Equal(1,await new OutboxRelay(db,new InMemoryBus(consumer)).RelayAsync());Assert.Equal(10m,consumer.SpendByCustomer["c1"]);Assert.NotNull(db.Outbox[0].PublishedAt);}
 [Fact] public void Consumer_is_idempotent(){var c=new LoyaltyConsumer();var e=new OrderPlaced(Guid.NewGuid(),Guid.NewGuid(),"c1",10m);Assert.True(c.Handle(e));Assert.False(c.Handle(e));Assert.Equal(10m,c.SpendByCustomer["c1"]);}
 [Fact] public async Task Completed_outbox_is_not_republished(){var db=new OrderDatabase();var c=new LoyaltyConsumer();db.Place("c1",10m);var relay=new OutboxRelay(db,new InMemoryBus(c));await relay.RelayAsync();Assert.Equal(0,await relay.RelayAsync());}
}
