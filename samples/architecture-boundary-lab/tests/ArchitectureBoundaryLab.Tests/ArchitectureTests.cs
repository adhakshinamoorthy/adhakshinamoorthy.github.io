using ArchitectureBoundaryLab.Application;
using ArchitectureBoundaryLab.Domain;
using ArchitectureBoundaryLab.Infrastructure;
using Xunit;

public sealed class ArchitectureTests
{
    [Fact]
    public void Domain_has_no_outward_project_dependency()
    {
        var references = typeof(Account).Assembly.GetReferencedAssemblies().Select(reference => reference.Name);
        Assert.DoesNotContain("ArchitectureBoundaryLab.Application", references);
        Assert.DoesNotContain("ArchitectureBoundaryLab.Infrastructure", references);
    }

    [Fact]
    public void Application_has_no_infrastructure_dependency()
    {
        var references = typeof(OpenAccountHandler).Assembly.GetReferencedAssemblies().Select(reference => reference.Name);
        Assert.DoesNotContain("ArchitectureBoundaryLab.Infrastructure", references);
    }

    [Fact]
    public void Application_ports_follow_store_naming_convention()
    {
        var ports = typeof(IAccountStore).Assembly.GetExportedTypes().Where(type => type.IsInterface);
        Assert.All(ports, port => Assert.EndsWith("Store", port.Name, StringComparison.Ordinal));
    }

    [Fact]
    public void Infrastructure_adapters_are_sealed()
    {
        var adapters = typeof(InMemoryAccountStore).Assembly.GetExportedTypes().Where(type => type.IsClass);
        Assert.All(adapters, adapter => Assert.True(adapter.IsSealed, $"{adapter.FullName} must be sealed."));
    }

    [Fact]
    public async Task Composition_preserves_application_port_behavior()
    {
        var store = new InMemoryAccountStore();
        var account = await new OpenAccountHandler(store).HandleAsync("Grace Hopper");
        Assert.Equal(account, await store.FindAsync(account.Id, default));
    }
}
