namespace DependencyInjectionLifetimes.Infrastructure;

public interface INotificationChannel
{
    string Name { get; }

    string CreateMessage(string customer, Guid orderId);
}

public sealed class EmailNotificationChannel : INotificationChannel
{
    public string Name => "email";

    public string CreateMessage(string customer, Guid orderId) =>
        $"Email to {customer}: order {orderId} is ready.";
}

public sealed class SmsNotificationChannel : INotificationChannel
{
    public string Name => "sms";

    public string CreateMessage(string customer, Guid orderId) =>
        $"SMS to {customer}: order {orderId} is ready.";
}
