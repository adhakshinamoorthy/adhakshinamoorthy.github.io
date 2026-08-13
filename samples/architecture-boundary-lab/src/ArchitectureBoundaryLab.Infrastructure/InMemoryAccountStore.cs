using System.Collections.Concurrent;
using ArchitectureBoundaryLab.Application;
using ArchitectureBoundaryLab.Domain;

namespace ArchitectureBoundaryLab.Infrastructure;

public sealed class InMemoryAccountStore : IAccountStore
{
    private readonly ConcurrentDictionary<Guid, Account> _accounts = new();

    public Task AddAsync(Account account, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _accounts[account.Id] = account;
        return Task.CompletedTask;
    }

    public Task<Account?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_accounts.GetValueOrDefault(id));
    }
}
