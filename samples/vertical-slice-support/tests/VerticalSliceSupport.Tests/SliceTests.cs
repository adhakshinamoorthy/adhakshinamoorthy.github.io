using VerticalSliceSupport;
using Xunit;

public sealed class SliceTests
{
    [Fact] public async Task Create_slice_owns_validation_mapping_and_behavior()
    {
        var store = new TicketStore();
        var result = await new CreateTicket.Handler(store).HandleAsync(new("customer-1", " Printer offline "), default);
        Assert.Equal("Printer offline", result.Subject); Assert.Equal(TicketStatus.Open, result.Status); Assert.NotNull(store.Find(result.Id));
    }

    [Fact] public async Task Invalid_request_does_not_mutate_state()
    {
        var store = new TicketStore();
        await Assert.ThrowsAsync<ValidationException>(async () => await new CreateTicket.Handler(store).HandleAsync(new("customer-1", ""), default));
        Assert.Empty(store.ForCustomer("customer-1"));
    }

    [Fact] public async Task Query_slice_does_not_disclose_another_customers_ticket()
    {
        var store = new TicketStore();
        var created = await new CreateTicket.Handler(store).HandleAsync(new("customer-a", "Private"), default);
        var result = await new GetTicket.Handler(store).HandleAsync(new("customer-b", created.Id), default);
        Assert.Null(result);
    }
}
