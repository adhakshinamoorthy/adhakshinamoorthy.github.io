using Testcontainers.PostgreSql;
using TestcontainersPostgresInventory;
using Xunit;

public sealed class InventoryPolicyTests
{
    [Fact]
    public void Container_image_is_explicitly_pinned() =>
        Assert.Equal("postgres:16.4-alpine", PostgresTestEnvironment.Image);

    [Fact]
    public void Inventory_rejects_negative_stock() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new InventoryItem("sku-1", -1));
}

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(PostgresTestEnvironment.Image)
        .WithDatabase("atlas_tests")
        .WithUsername("atlas")
        .WithPassword("local-test-password")
        .Build();

    public InventoryRepository Repository { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Repository = new InventoryRepository(_container.GetConnectionString());
        await Repository.EnsureSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        await Repository.DisposeAsync();
        await _container.DisposeAsync();
    }
}

public sealed class PostgreSqlIntegrationTests : IClassFixture<PostgreSqlFixture>
{
    private readonly InventoryRepository _repository;

    public PostgreSqlIntegrationTests(PostgreSqlFixture fixture) => _repository = fixture.Repository;

    [Fact]
    public async Task Real_postgres_persists_and_reads_inventory()
    {
        await _repository.ClearAsync();
        var expected = new InventoryItem("sku-real", 9);
        await _repository.UpsertAsync(expected);
        Assert.Equal(expected, await _repository.FindAsync(expected.Sku));
    }

    [Fact]
    public async Task Upsert_updates_existing_row_without_duplicate()
    {
        await _repository.ClearAsync();
        await _repository.UpsertAsync(new InventoryItem("sku-upsert", 1));
        await _repository.UpsertAsync(new InventoryItem("sku-upsert", 4));
        Assert.Equal(4, (await _repository.FindAsync("sku-upsert"))?.Available);
    }
}
