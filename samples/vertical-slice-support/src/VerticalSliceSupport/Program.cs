using VerticalSliceSupport;

var store = new TicketStore();
var created = await new CreateTicket.Handler(store).HandleAsync(new("customer-42", "Cannot download invoice"), default);
var found = await new GetTicket.Handler(store).HandleAsync(new("customer-42", created.Id), default);
Console.WriteLine($"{found?.Id}: {found?.Subject} [{found?.Status}]");
