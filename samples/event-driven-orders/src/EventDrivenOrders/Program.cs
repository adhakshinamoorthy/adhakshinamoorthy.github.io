using EventDrivenOrders;
var database = new OrderDatabase(); var loyalty = new LoyaltyConsumer();
database.Place("customer-42", 75m); await new OutboxRelay(database, new InMemoryBus(loyalty)).RelayAsync();
Console.WriteLine($"Customer spend: {loyalty.SpendByCustomer["customer-42"]:C}; pending={database.Outbox.Count(x => x.PublishedAt is null)}");
