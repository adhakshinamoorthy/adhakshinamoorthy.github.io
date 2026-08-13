using DesignPatternsPricing;
using Xunit;

public sealed class PatternTests
{
    [Fact] public void Strategy_varies_discount_without_branching_in_service()
    {
        var taxes = new FixedTaxGateway(0); var cart = new Cart("gold", 200m);
        Assert.Equal(20m, new PricingService(new LoyaltyDiscount(), new LegacyTaxAdapter(taxes)).Quote(cart, "IN").Discount);
        Assert.Equal(0m, new PricingService(new StandardDiscount(), new LegacyTaxAdapter(taxes)).Quote(cart, "IN").Discount);
    }

    [Fact] public void Adapter_translates_legacy_basis_points_contract()
    {
        Assert.Equal(18m, new LegacyTaxAdapter(new FixedTaxGateway(1800)).CalculateTax(100m, "IN"));
    }

    [Fact] public void Decorator_adds_policy_without_changing_pricing_service()
    {
        var service = new MaximumTotalDecorator(new PricingService(new StandardDiscount(), new LegacyTaxAdapter(new FixedTaxGateway(0))), 100m);
        Assert.Throws<InvalidOperationException>(() => service.Quote(new("standard", 101m), "IN"));
    }

    [Fact] public void Factory_composes_tier_specific_strategy_and_decorator()
    {
        var quote = PricingFactory.Create("gold", new LegacyTaxAdapter(new FixedTaxGateway(0))).Quote(new("gold", 200m), "IN");
        Assert.Equal(180m, quote.Total); Assert.Equal(["loyalty-10", "maximum-total"], quote.AppliedRules);
    }

    private sealed class FixedTaxGateway(int rate) : ILegacyTaxGateway { public int TaxInBasisPoints(string region) => rate; }
}
