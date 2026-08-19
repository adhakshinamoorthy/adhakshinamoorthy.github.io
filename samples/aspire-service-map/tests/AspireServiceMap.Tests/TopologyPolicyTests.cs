using AspireServiceMap.Api;
using Xunit;

public sealed class TopologyPolicyTests
{
    [Fact]
    public void Known_directed_reference_is_valid()
    {
        var errors = TopologyPolicy.Validate(["catalog", "orders"], [new("orders", "catalog")]);
        Assert.Empty(errors);
    }

    [Fact]
    public void Missing_resource_is_reported()
    {
        var errors = TopologyPolicy.Validate(["orders"], [new("orders", "catalog")]);
        Assert.Single(errors);
    }

    [Fact]
    public void Self_reference_is_reported()
    {
        var errors = TopologyPolicy.Validate(["orders"], [new("orders", "orders")]);
        Assert.Single(errors);
    }
}
