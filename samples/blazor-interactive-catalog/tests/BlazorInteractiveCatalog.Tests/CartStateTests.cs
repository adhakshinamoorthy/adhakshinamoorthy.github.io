using BlazorInteractiveCatalog.Models;
using BlazorInteractiveCatalog.Services;
using Xunit;

namespace BlazorInteractiveCatalog.Tests;

public sealed class CartStateTests
{
    private static readonly Product Guide = new(7, "Guide", "Book", "A test product", 12.50m);

    [Fact]
    public void Add_NewProductCreatesOneLine()
    {
        var cart = new CartState();

        cart.Add(Guide);

        var line = Assert.Single(cart.Lines);
        Assert.Equal(1, line.Quantity);
        Assert.Equal(12.50m, cart.Total);
    }

    [Fact]
    public void Add_ExistingProductIncrementsQuantity()
    {
        var cart = new CartState();

        cart.Add(Guide);
        cart.Add(Guide);

        Assert.Equal(2, cart.TotalItems);
        Assert.Equal(25.00m, cart.Total);
    }

    [Fact]
    public void SeparateScopedInstancesDoNotShareState()
    {
        var firstCircuit = new CartState();
        var secondCircuit = new CartState();

        firstCircuit.Add(Guide);

        Assert.Equal(1, firstCircuit.TotalItems);
        Assert.Equal(0, secondCircuit.TotalItems);
    }

    [Fact]
    public void Clear_RemovesAllLines()
    {
        var cart = new CartState();
        cart.Add(Guide);

        cart.Clear();

        Assert.Empty(cart.Lines);
        Assert.Equal(0m, cart.Total);
    }
}
