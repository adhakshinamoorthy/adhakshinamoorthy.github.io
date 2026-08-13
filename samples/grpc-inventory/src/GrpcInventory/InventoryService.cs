using Grpc.Core;
using GrpcInventory.Contracts;

namespace GrpcInventory;

internal sealed class InventoryService(InventoryStore store) : Inventory.InventoryBase
{
    public override Task<StockReply> GetStock(GetStockRequest request, ServerCallContext context)
    {
        var sku = Normalize(request.Sku);
        if (!store.TryGet(sku, out var stock))
            throw new RpcException(new Status(StatusCode.NotFound, "The SKU was not found."));
        return Task.FromResult(ToReply(sku, stock));
    }

    public override async Task WatchStock(WatchStockRequest request, IServerStreamWriter<StockReply> responseStream, ServerCallContext context)
    {
        if (request.Skus.Count is 0 or > 20)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Supply between 1 and 20 SKUs."));

        foreach (var rawSku in request.Skus)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var sku = Normalize(rawSku);
            if (store.TryGet(sku, out var stock)) await responseStream.WriteAsync(ToReply(sku, stock), context.CancellationToken);
        }
    }

    private static string Normalize(string sku)
    {
        var value = sku.Trim().ToUpperInvariant();
        if (value.Length is < 3 or > 40)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "SKU length must be between 3 and 40 characters."));
        return value;
    }

    private static StockReply ToReply(string sku, (int Available, long Version) stock) =>
        new() { Sku = sku, Available = stock.Available, Version = stock.Version };
}
