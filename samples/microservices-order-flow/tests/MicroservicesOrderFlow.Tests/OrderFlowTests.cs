using MicroservicesOrderFlow;
using Xunit;

public sealed class OrderFlowTests
{
    [Fact]
    public void Placing_an_order_writes_order_and_outbox_together()
    {
        var service = new OrderService();
        var order = service.Place(new("customer-7", [new("sku-1", 2)]));

        Assert.Equal(order.Id, service.Find(order.Id)?.Id);
        Assert.Equal(order.Id, service.DequeueOutbox()?.OrderId);
    }

    [Fact]
    public void Consumer_ignores_redelivery()
    {
        var consumer = new InventoryConsumer();
        var message = new OrderPlaced(Guid.NewGuid(), Guid.NewGuid(), "customer-7", [new("sku-1", 2)]);

        Assert.True(consumer.Handle(message));
        Assert.False(consumer.Handle(message));
        Assert.Equal(2, consumer.Reserved("sku-1"));
    }

    [Fact]
    public void Invalid_order_is_rejected_before_state_changes()
    {
        var service = new OrderService();
        Assert.Throws<ArgumentException>(() => service.Place(new("customer-7", [])));
        Assert.Null(service.DequeueOutbox());
    }
}
