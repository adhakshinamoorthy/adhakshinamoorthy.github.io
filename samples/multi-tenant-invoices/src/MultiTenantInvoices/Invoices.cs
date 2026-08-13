using System.Collections.Concurrent;

namespace MultiTenantInvoices;

public sealed record Invoice(Guid Id, string Number, decimal Amount);
public sealed record CreateInvoiceRequest(string Number, decimal Amount);
internal sealed record StoredInvoice(Guid Id, string TenantId, string Number, decimal Amount);

internal sealed class InvoiceStore
{
    private readonly ConcurrentDictionary<Guid, StoredInvoice> _items = new(new[]
    {
        Pair("00000000-0000-0000-0000-000000000001", "tenant-a", "A-100", 125m),
        Pair("00000000-0000-0000-0000-000000000002", "tenant-b", "B-200", 275m)
    });
    private static KeyValuePair<Guid, StoredInvoice> Pair(string id, string tenant, string number, decimal amount)
    { var key = Guid.Parse(id); return new(key, new(key, tenant, number, amount)); }
    public IReadOnlyList<StoredInvoice> List(string tenant) => _items.Values.Where(item => item.TenantId == tenant).OrderBy(item => item.Number).ToArray();
    public StoredInvoice? Find(string tenant, Guid id) => _items.TryGetValue(id, out var item) && item.TenantId == tenant ? item : null;
    public StoredInvoice Add(string tenant, CreateInvoiceRequest request)
    { var item = new StoredInvoice(Guid.NewGuid(), tenant, request.Number.Trim(), request.Amount); _items[item.Id] = item; return item; }
}

internal sealed class TenantInvoiceRepository(TenantContext tenant, InvoiceStore store)
{
    private string Id => tenant.IsSet ? tenant.TenantId.Value : throw new InvalidOperationException("Tenant context is unavailable.");
    public IReadOnlyList<Invoice> List() => store.List(Id).Select(Map).ToArray();
    public Invoice? Find(Guid id) => store.Find(Id, id) is { } item ? Map(item) : null;
    public Invoice Add(CreateInvoiceRequest request) => Map(store.Add(Id, request));
    private static Invoice Map(StoredInvoice item) => new(item.Id, item.Number, item.Amount);
}

internal sealed class TenantQuota
{
    private readonly ConcurrentDictionary<string, int> _creates = new(StringComparer.Ordinal);
    public bool TryConsume(string tenant) => _creates.AddOrUpdate(tenant, 1, (_, current) => current + 1) <= 2;
}
