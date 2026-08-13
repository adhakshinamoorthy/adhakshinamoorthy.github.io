namespace CqrsOrdersMediator;

public interface IRequest<out TResponse>;
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse> { ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken); }
public delegate ValueTask<TResponse> RequestHandlerDelegate<TResponse>();
public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : IRequest<TResponse> { ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken); }

public sealed class Mediator
{
    public ValueTask<TResponse> SendAsync<TRequest, TResponse>(TRequest request, IRequestHandler<TRequest, TResponse> handler, IReadOnlyList<IPipelineBehavior<TRequest, TResponse>> behaviors, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>
    {
        RequestHandlerDelegate<TResponse> pipeline = () => handler.HandleAsync(request, cancellationToken);
        for (var index = behaviors.Count - 1; index >= 0; index--)
        {
            var next = pipeline; var behavior = behaviors[index];
            pipeline = () => behavior.HandleAsync(request, next, cancellationToken);
        }
        return pipeline();
    }
}

public sealed record PlaceOrder(string RequestId, string CustomerId, decimal Total) : IRequest<PlaceOrderResult>;
public sealed record PlaceOrderResult(Guid OrderId, bool Replayed);
public sealed record GetOrder(Guid OrderId) : IRequest<OrderView?>;
public sealed record OrderView(Guid Id, string CustomerId, decimal Total, string Status);

public sealed class OrderWriteStore
{
    private readonly Dictionary<Guid, (string CustomerId, decimal Total)> orders = [];
    public void Add(Guid id, string customerId, decimal total) => orders.Add(id, (customerId, total));
}

public sealed class OrderReadStore
{
    private readonly Dictionary<Guid, OrderView> orders = [];
    public void Project(OrderView view) => orders[view.Id] = view;
    public OrderView? Find(Guid id) => orders.GetValueOrDefault(id);
}

public sealed class PlaceOrderHandler(OrderWriteStore writes, OrderReadStore reads) : IRequestHandler<PlaceOrder, PlaceOrderResult>
{
    public ValueTask<PlaceOrderResult> HandleAsync(PlaceOrder request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var id = Guid.NewGuid(); writes.Add(id, request.CustomerId, request.Total);
        reads.Project(new(id, request.CustomerId, request.Total, "Accepted"));
        return ValueTask.FromResult(new PlaceOrderResult(id, false));
    }
}

public sealed class GetOrderHandler(OrderReadStore reads) : IRequestHandler<GetOrder, OrderView?>
{
    public ValueTask<OrderView?> HandleAsync(GetOrder request, CancellationToken cancellationToken) { cancellationToken.ThrowIfCancellationRequested(); return ValueTask.FromResult(reads.Find(request.OrderId)); }
}

public sealed class PlaceOrderValidation : IPipelineBehavior<PlaceOrder, PlaceOrderResult>
{
    public ValueTask<PlaceOrderResult> HandleAsync(PlaceOrder request, RequestHandlerDelegate<PlaceOrderResult> next, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RequestId)) throw new ValidationException("RequestId is required.");
        if (string.IsNullOrWhiteSpace(request.CustomerId)) throw new ValidationException("CustomerId is required.");
        if (request.Total is <= 0 or > 100_000) throw new ValidationException("Total must be between 0 and 100000.");
        return next();
    }
}

public sealed class PlaceOrderIdempotency : IPipelineBehavior<PlaceOrder, PlaceOrderResult>
{
    private readonly Dictionary<string, PlaceOrderResult> results = new(StringComparer.Ordinal);
    public async ValueTask<PlaceOrderResult> HandleAsync(PlaceOrder request, RequestHandlerDelegate<PlaceOrderResult> next, CancellationToken cancellationToken)
    {
        if (results.TryGetValue(request.RequestId, out var result)) return result with { Replayed = true };
        result = await next(); results.Add(request.RequestId, result); return result;
    }
}

public sealed class ValidationException(string message) : Exception(message);
