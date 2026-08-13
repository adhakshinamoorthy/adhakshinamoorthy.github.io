using ModularMonolithStorefront;

var catalog = new CatalogModule();
catalog.Seed(new("BOOK-1", "Distributed Systems", 39.95m, 5));
var events = new InProcessOrderEvents();
var order = new OrdersModule(catalog, catalog, events).Place("BOOK-1", 2);
Console.WriteLine($"Accepted {order.OrderId}: {order.Quantity} x {order.Sku} = {order.Total:C}");
