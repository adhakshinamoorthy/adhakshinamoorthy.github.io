using ArchitectureBoundaryLab.Domain;

namespace ArchitectureBoundaryLab.Application;

public interface IAccountStore
{
    Task AddAsync(Account account, CancellationToken cancellationToken);
    Task<Account?> FindAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class OpenAccountHandler(IAccountStore store)
{
    public async Task<Account> HandleAsync(string owner, CancellationToken cancellationToken = default)
    {
        var account = new Account(Guid.NewGuid(), owner);
        await store.AddAsync(account, cancellationToken);
        return account;
    }
}
