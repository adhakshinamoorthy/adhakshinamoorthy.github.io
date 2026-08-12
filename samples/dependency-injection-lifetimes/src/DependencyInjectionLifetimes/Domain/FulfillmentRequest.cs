namespace DependencyInjectionLifetimes.Domain;

public sealed record FulfillmentRequest(
    Guid OrderId,
    string Customer,
    int Quantity,
    NotificationKind Notification);

public enum NotificationKind
{
    Email,
    Sms
}

public sealed record FulfillmentReceipt(
    Guid OrderId,
    string Channel,
    string Message,
    Guid ApplicationId,
    Guid ScopeId,
    Guid ActivityId);
