using DependencyInjectionLifetimes.Application;
using DependencyInjectionLifetimes.Composition;
using DependencyInjectionLifetimes.Configuration;
using DependencyInjectionLifetimes.Domain;
using DependencyInjectionLifetimes.Infrastructure;
using DependencyInjectionLifetimes.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace DependencyInjectionLifetimes.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void Lifetimes_ExpressExpectedOwnership()
    {
        using var provider = CreateProvider();

        var singletonFromRoot = provider.GetRequiredService<ApplicationIdentity>();
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        Assert.Same(singletonFromRoot, firstScope.ServiceProvider.GetRequiredService<ApplicationIdentity>());
        Assert.Same(
            firstScope.ServiceProvider.GetRequiredService<OperationScope>(),
            firstScope.ServiceProvider.GetRequiredService<OperationScope>());
        Assert.NotSame(
            firstScope.ServiceProvider.GetRequiredService<OperationScope>(),
            secondScope.ServiceProvider.GetRequiredService<OperationScope>());
        Assert.NotSame(
            firstScope.ServiceProvider.GetRequiredService<ActivityIdentity>(),
            firstScope.ServiceProvider.GetRequiredService<ActivityIdentity>());
    }

    [Fact]
    public void KeyedServices_ResolveNamedImplementations()
    {
        using var provider = CreateProvider();

        var email = provider.GetRequiredKeyedService<INotificationChannel>("email");
        var sms = provider.GetRequiredKeyedService<INotificationChannel>("sms");

        Assert.IsType<EmailNotificationChannel>(email);
        Assert.IsType<SmsNotificationChannel>(sms);
    }

    [Fact]
    public async Task Processor_UsesRequestedChannelAndScopeIdentity()
    {
        using var provider = CreateProvider();
        await using var scope = provider.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<FulfillmentProcessor>();
        var request = new FulfillmentRequest(Guid.NewGuid(), "+1-555-0100", 2, NotificationKind.Sms);

        var receipt = await processor.ProcessAsync(request);

        Assert.Equal("sms", receipt.Channel);
        Assert.Contains(request.OrderId.ToString(), receipt.Message, StringComparison.Ordinal);
        Assert.Equal(scope.ServiceProvider.GetRequiredService<OperationScope>().Id, receipt.ScopeId);
    }

    [Fact]
    public async Task SeparateScopes_IsolateOperationStateButShareApplicationIdentity()
    {
        using var provider = CreateProvider();
        var first = await ProcessInNewScopeAsync(provider);
        var second = await ProcessInNewScopeAsync(provider);

        Assert.Equal(first.ApplicationId, second.ApplicationId);
        Assert.NotEqual(first.ScopeId, second.ScopeId);
        Assert.NotEqual(first.ActivityId, second.ActivityId);
    }

    [Fact]
    public void DisposingScope_DisposesOwnedScopedService()
    {
        using var provider = CreateProvider();
        var scope = provider.CreateScope();
        var operation = scope.ServiceProvider.GetRequiredService<OperationScope>();

        scope.Dispose();

        Assert.True(operation.IsDisposed);
    }

    [Fact]
    public void InvalidOptions_AreRejectedWhenEvaluated()
    {
        var values = new Dictionary<string, string?>
        {
            ["Fulfillment:MaximumQuantity"] = "0",
            ["Fulfillment:DefaultChannel"] = "carrier-pigeon"
        };
        using var provider = CreateProvider(values);

        Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<FulfillmentOptions>>().Value);
    }

    private static ServiceProvider CreateProvider(IReadOnlyDictionary<string, string?>? values = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values ?? new Dictionary<string, string?>
            {
                ["Fulfillment:MaximumQuantity"] = "10",
                ["Fulfillment:DefaultChannel"] = "email"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddFulfillment(configuration);

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private static async Task<FulfillmentReceipt> ProcessInNewScopeAsync(IServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<FulfillmentProcessor>();
        return await processor.ProcessAsync(
            new(Guid.NewGuid(), "customer@example.com", 1, NotificationKind.Email));
    }
}
