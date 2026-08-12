using DependencyInjectionLifetimes.Application;
using DependencyInjectionLifetimes.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionLifetimes.Runtime;

public sealed class FulfillmentRunner(IServiceScopeFactory scopeFactory)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var requests = new[]
        {
            new FulfillmentRequest(
                Guid.Parse("10000000-0000-0000-0000-000000000001"),
                "ada@example.com",
                2,
                NotificationKind.Email),
            new FulfillmentRequest(
                Guid.Parse("10000000-0000-0000-0000-000000000002"),
                "+1-555-0100",
                1,
                NotificationKind.Sms)
        };

        foreach (var request in requests)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<FulfillmentProcessor>();
            var receipt = await processor.ProcessAsync(request, cancellationToken);

            Console.WriteLine(
                "{0} | app={1} scope={2} activity={3} | {4}",
                receipt.Channel,
                receipt.ApplicationId,
                receipt.ScopeId,
                receipt.ActivityId,
                receipt.Message);
        }
    }
}
