using DesignPatternsPricing;

var taxes = new LegacyTaxAdapter(new DemoTaxGateway());
var quote = PricingFactory.Create("gold", taxes).Quote(new("gold", 150m), "IN");
Console.WriteLine($"Subtotal={quote.Subtotal:C}, discount={quote.Discount:C}, tax={quote.Tax:C}, total={quote.Total:C}");

sealed class DemoTaxGateway : ILegacyTaxGateway { public int TaxInBasisPoints(string region) => region == "IN" ? 1800 : 0; }
