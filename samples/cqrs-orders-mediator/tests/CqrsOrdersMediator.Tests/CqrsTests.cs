using CqrsOrdersMediator;
using Xunit;

public sealed class CqrsTests
{
    [Fact] public async Task Command_updates_write_model_and_projection()
    {
        var mediator = new Mediator(); var reads = new OrderReadStore();
        var result = await mediator.SendAsync(new PlaceOrder("r1", "c1", 25m), new PlaceOrderHandler(new(), reads), [new PlaceOrderValidation(), new PlaceOrderIdempotency()]);
        var view = await mediator.SendAsync(new GetOrder(result.OrderId), new GetOrderHandler(reads), []);
        Assert.Equal(25m, view!.Total); Assert.Equal("Accepted", view.Status);
    }

    [Fact] public async Task Pipeline_rejects_invalid_command_before_handler()
    {
        var mediator = new Mediator(); var reads = new OrderReadStore();
        await Assert.ThrowsAsync<ValidationException>(async () => await mediator.SendAsync(new PlaceOrder("r1", "c1", -1m), new PlaceOrderHandler(new(), reads), [new PlaceOrderValidation()]));
        Assert.Null(reads.Find(Guid.NewGuid()));
    }

    [Fact] public async Task Idempotency_replays_result_without_duplicate_order()
    {
        var mediator = new Mediator(); var reads = new OrderReadStore(); var idempotency = new PlaceOrderIdempotency(); var handler = new PlaceOrderHandler(new(), reads);
        var first = await mediator.SendAsync(new PlaceOrder("same", "c1", 25m), handler, [new PlaceOrderValidation(), idempotency]);
        var second = await mediator.SendAsync(new PlaceOrder("same", "c1", 25m), handler, [new PlaceOrderValidation(), idempotency]);
        Assert.Equal(first.OrderId, second.OrderId); Assert.False(first.Replayed); Assert.True(second.Replayed);
    }

    [Fact] public async Task Query_for_unknown_id_returns_null()
    {
        var result = await new Mediator().SendAsync(new GetOrder(Guid.NewGuid()), new GetOrderHandler(new()), []);
        Assert.Null(result);
    }
}
