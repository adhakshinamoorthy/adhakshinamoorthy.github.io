using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcInventory;

internal sealed class ApiKeyInterceptor(IConfiguration configuration) : Interceptor
{
    private readonly string _expected = configuration["GrpcApiKey"]
        ?? throw new InvalidOperationException("GrpcApiKey is required.");

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        RequireKey(context);
        return continuation(request, context);
    }

    public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        RequireKey(context);
        return continuation(request, responseStream, context);
    }

    private void RequireKey(ServerCallContext context)
    {
        var supplied = context.RequestHeaders.GetValue("x-api-key");
        if (!string.Equals(supplied, _expected, StringComparison.Ordinal))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Valid credentials are required."));
    }
}
