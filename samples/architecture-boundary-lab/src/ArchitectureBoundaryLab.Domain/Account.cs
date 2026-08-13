namespace ArchitectureBoundaryLab.Domain;

public sealed record Account
{
    public Account(Guid id, string owner)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        Id = id;
        Owner = owner;
    }

    public Guid Id { get; }
    public string Owner { get; }
}
