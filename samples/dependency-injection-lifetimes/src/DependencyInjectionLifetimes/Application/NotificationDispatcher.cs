using DependencyInjectionLifetimes.Domain;
using DependencyInjectionLifetimes.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionLifetimes.Application;

public sealed class NotificationDispatcher(
    [FromKeyedServices("email")] INotificationChannel email,
    [FromKeyedServices("sms")] INotificationChannel sms)
{
    public INotificationChannel Select(NotificationKind kind) => kind switch
    {
        NotificationKind.Email => email,
        NotificationKind.Sms => sms,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported channel.")
    };
}
