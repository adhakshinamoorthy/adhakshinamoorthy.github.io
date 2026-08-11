using EfCoreOrderManagement.Domain;
using EfCoreOrderManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCoreOrderManagement.Application;

public sealed record PlaceOrderRequest(
    string CustomerEmail,
    string CustomerName,
    Address ShippingAddress,
    IReadOnlyCollection<OrderLineInput> Lines);

public sealed class OrderService(OrderingDbContext db)
{
    public async Task<Guid> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var normalizedEmail = request.CustomerEmail.Trim().ToUpperInvariant();

        var customer = await db.Customers
            .SingleOrDefaultAsync(item => item.Email == normalizedEmail, cancellationToken);

        if (customer is null)
        {
            customer = Customer.Create(request.CustomerEmail, request.CustomerName);
            db.Customers.Add(customer);
            await db.SaveChangesAsync(cancellationToken);
        }

        var order = Order.Place(customer.Id, request.ShippingAddress, request.Lines, DateTime.UtcNow);
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return order.Id;
    }
}
