using DependencyInjectionLifetimes.Configuration;
using DependencyInjectionLifetimes.Domain;
using DependencyInjectionLifetimes.Infrastructure;
using DependencyInjectionLifetimes.Services;
using Microsoft.Extensions.Options;

namespace DependencyInjectionLifetimes.Application;

public sealed class FulfillmentProcessor(
    IOrderRepository repository,
    NotificationDispatcher notifications,
    IOptions<FulfillmentOptions> options,
    ApplicationIdentity application,
    OperationScope operation,
    ActivityIdentity activity)
{
    public async Task<FulfillmentReceipt> ProcessAsync(
        FulfillmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Quantity is <= 0 || request.Quantity > options.Value.MaximumQuantity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                $"Quantity must be between 1 and {options.Value.MaximumQuantity}.");
        }

        await repository.SaveAsync(request, cancellationToken);
        var channel = notifications.Select(request.Notification);

        return new(
            request.OrderId,
            channel.Name,
            channel.CreateMessage(request.Customer, request.OrderId),
            application.Id,
            operation.Id,
            activity.Id);
    }
}
