namespace VerticalSliceSupport;

public sealed record Ticket(Guid Id, string CustomerId, string Subject, TicketStatus Status);
public enum TicketStatus { Open, Closed }

public sealed class TicketStore
{
    private readonly Dictionary<Guid, Ticket> tickets = [];
    public void Add(Ticket ticket) => tickets.Add(ticket.Id, ticket);
    public Ticket? Find(Guid id) => tickets.GetValueOrDefault(id);
    public IReadOnlyList<Ticket> ForCustomer(string customerId) => tickets.Values.Where(ticket => ticket.CustomerId == customerId).ToArray();
}

public interface IRequest<out TResponse>;
public interface IHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse> { ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken); }

public static class CreateTicket
{
    public sealed record Command(string CustomerId, string Subject) : IRequest<Result>;
    public sealed record Result(Guid Id, string Subject, TicketStatus Status);

    public sealed class Handler(TicketStore store) : IHandler<Command, Result>
    {
        public ValueTask<Result> HandleAsync(Command request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(request.CustomerId)) throw new ValidationException("CustomerId is required.");
            if (string.IsNullOrWhiteSpace(request.Subject) || request.Subject.Length > 120) throw new ValidationException("Subject must contain 1 to 120 characters.");
            var ticket = new Ticket(Guid.NewGuid(), request.CustomerId.Trim(), request.Subject.Trim(), TicketStatus.Open);
            store.Add(ticket);
            return ValueTask.FromResult(new Result(ticket.Id, ticket.Subject, ticket.Status));
        }
    }
}

public static class GetTicket
{
    public sealed record Query(string CustomerId, Guid Id) : IRequest<Result?>;
    public sealed record Result(Guid Id, string Subject, TicketStatus Status);

    public sealed class Handler(TicketStore store) : IHandler<Query, Result?>
    {
        public ValueTask<Result?> HandleAsync(Query request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ticket = store.Find(request.Id);
            return ValueTask.FromResult(ticket is null || ticket.CustomerId != request.CustomerId ? null : new Result(ticket.Id, ticket.Subject, ticket.Status));
        }
    }
}

public sealed class ValidationException(string message) : Exception(message);
