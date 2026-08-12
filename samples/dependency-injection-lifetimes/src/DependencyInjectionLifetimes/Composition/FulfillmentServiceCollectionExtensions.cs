using DependencyInjectionLifetimes.Application;
using DependencyInjectionLifetimes.Configuration;
using DependencyInjectionLifetimes.Infrastructure;
using DependencyInjectionLifetimes.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionLifetimes.Composition;

public static class FulfillmentServiceCollectionExtensions
{
    public static IServiceCollection AddFulfillment(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<FulfillmentOptions>()
            .Bind(configuration.GetSection(FulfillmentOptions.SectionName))
            .Validate(
                options => options.MaximumQuantity is > 0 and <= 1_000,
                "MaximumQuantity must be between 1 and 1000.")
            .Validate(
                options => options.DefaultChannel is "email" or "sms",
                "DefaultChannel must be email or sms.")
            .ValidateOnStart();

        services.AddSingleton<ApplicationIdentity>();
        services.AddScoped<OperationScope>();
        services.AddTransient<ActivityIdentity>();

        services.AddScoped<IOrderRepository, InMemoryOrderRepository>();
        services.AddKeyedSingleton<INotificationChannel, EmailNotificationChannel>("email");
        services.AddKeyedSingleton<INotificationChannel, SmsNotificationChannel>("sms");
        services.AddScoped<NotificationDispatcher>();
        services.AddScoped<FulfillmentProcessor>();

        return services;
    }
}
