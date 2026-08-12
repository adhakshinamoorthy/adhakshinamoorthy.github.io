namespace DependencyInjectionLifetimes.Configuration;

public sealed class FulfillmentOptions
{
    public const string SectionName = "Fulfillment";

    public int MaximumQuantity { get; init; } = 10;

    public string DefaultChannel { get; init; } = "email";
}
