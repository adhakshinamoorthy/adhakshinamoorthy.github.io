using CqrsOrdersMediator;

var mediator = new Mediator(); var writes = new OrderWriteStore(); var reads = new OrderReadStore();
var placed = await mediator.SendAsync(new PlaceOrder("request-1", "customer-42", 75m), new PlaceOrderHandler(writes, reads), [new PlaceOrderValidation(), new PlaceOrderIdempotency()]);
var order = await mediator.SendAsync(new GetOrder(placed.OrderId), new GetOrderHandler(reads), []);
Console.WriteLine($"{order?.Id}: {order?.Status}, total={order?.Total:C}");
