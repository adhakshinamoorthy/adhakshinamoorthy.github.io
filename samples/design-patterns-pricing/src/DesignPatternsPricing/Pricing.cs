namespace DesignPatternsPricing;

public sealed record Cart(string CustomerTier, decimal Subtotal);
public sealed record PriceQuote(decimal Subtotal, decimal Discount, decimal Tax, decimal Total, IReadOnlyList<string> AppliedRules);

public interface IDiscountStrategy { decimal Calculate(Cart cart); string Name { get; } }
public sealed class StandardDiscount : IDiscountStrategy { public decimal Calculate(Cart cart) => 0m; public string Name => "standard"; }
public sealed class LoyaltyDiscount : IDiscountStrategy { public decimal Calculate(Cart cart) => cart.Subtotal >= 100m ? decimal.Round(cart.Subtotal * 0.10m, 2) : 0m; public string Name => "loyalty-10"; }

public interface ITaxProvider { decimal CalculateTax(decimal taxableAmount, string region); }
public interface ILegacyTaxGateway { int TaxInBasisPoints(string region); }
public sealed class LegacyTaxAdapter(ILegacyTaxGateway gateway) : ITaxProvider
{
    public decimal CalculateTax(decimal taxableAmount, string region) => decimal.Round(taxableAmount * gateway.TaxInBasisPoints(region) / 10_000m, 2);
}

public interface IPricingService { PriceQuote Quote(Cart cart, string region); }
public sealed class PricingService(IDiscountStrategy discounts, ITaxProvider taxes) : IPricingService
{
    public PriceQuote Quote(Cart cart, string region)
    {
        if (cart.Subtotal <= 0) throw new ArgumentOutOfRangeException(nameof(cart));
        var discount = discounts.Calculate(cart); var taxable = cart.Subtotal - discount; var tax = taxes.CalculateTax(taxable, region);
        return new(cart.Subtotal, discount, tax, taxable + tax, [discounts.Name]);
    }
}

public sealed class MaximumTotalDecorator(IPricingService inner, decimal maximum) : IPricingService
{
    public PriceQuote Quote(Cart cart, string region)
    {
        var quote = inner.Quote(cart, region);
        if (quote.Total > maximum) throw new InvalidOperationException($"Quote exceeds the {maximum:C} safety limit.");
        return quote with { AppliedRules = [.. quote.AppliedRules, "maximum-total"] };
    }
}

public static class PricingFactory
{
    public static IPricingService Create(string customerTier, ITaxProvider taxes, decimal maximum = 10_000m)
    {
        IDiscountStrategy strategy = customerTier.Equals("gold", StringComparison.OrdinalIgnoreCase) ? new LoyaltyDiscount() : new StandardDiscount();
        return new MaximumTotalDecorator(new PricingService(strategy, taxes), maximum);
    }
}
