using OrleansShoppingCart;
using Xunit;

public sealed class CartRulesTests
{
    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(2, 3, 5)]
    public void Merge_adds_valid_quantities(int current, int added, int expected) => Assert.Equal(expected, CartRules.Merge(current, added));

    [Theory]
    [InlineData(0)]
    [InlineData(26)]
    public void Merge_rejects_out_of_range_quantities(int quantity) => Assert.Throws<ArgumentOutOfRangeException>(() => CartRules.Merge(0, quantity));
}
