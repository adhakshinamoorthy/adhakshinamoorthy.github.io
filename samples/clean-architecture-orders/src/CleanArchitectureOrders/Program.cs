using CleanArchitectureOrders.Application;
using CleanArchitectureOrders.Infrastructure;

var handler = new PlaceOrderHandler(new InMemoryOrderRepository(), new SystemClock());
var receipt = await handler.HandleAsync(new("customer-42", [new("BOOK-1", 2, 24.95m)]));
Console.WriteLine($"Accepted {receipt.OrderId}: {receipt.Total:C} at {receipt.AcceptedAt:O}");
