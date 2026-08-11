namespace EfCoreOrderManagement.Domain;

public sealed class Customer
{
    private readonly List<Order> _orders = [];

    private Customer()
    {
    }

    private Customer(Guid id, string email, string name)
    {
        Id = id;
        Email = NormalizeEmail(email);
        Name = Require(name, nameof(name));
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<Order> Orders => _orders;

    public static Customer Create(string email, string name) => new(Guid.NewGuid(), email, name);

    private static string NormalizeEmail(string value) =>
        Require(value, nameof(value)).ToUpperInvariant();

    private static string Require(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value.Trim();
}
