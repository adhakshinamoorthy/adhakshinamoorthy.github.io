using System.Runtime.CompilerServices;
using CSharpLanguageWorkbench.Domain;

namespace CSharpLanguageWorkbench.Infrastructure;

public static class InMemoryOrderSource
{
    public static async IAsyncEnumerable<Order> ReadAsync(
        IEnumerable<Order> orders,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var order in orders)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return order;
        }
    }
}
