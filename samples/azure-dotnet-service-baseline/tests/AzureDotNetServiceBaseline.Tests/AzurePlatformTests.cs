using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using AzureDotNetServiceBaseline;
using Xunit;

namespace AzureDotNetServiceBaseline.Tests;

public sealed class AzurePlatformTests
{
    [Fact]
    public void Configuration_requires_https_endpoint()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Azure:StorageEndpoint"] = "http://storage.example"
        }).Build();

        Assert.Throws<InvalidOperationException>(() => AzurePlatformOptions.FromConfiguration(configuration));
    }

    [Fact]
    public void Production_uses_managed_identity()
    {
        var options = new AzurePlatformOptions(new Uri("https://example.blob.core.windows.net"), null);
        var credential = AzureCredentialFactory.Create(new TestEnvironment("Production"), options);
        Assert.IsType<ManagedIdentityCredential>(credential);
    }

    [Fact]
    public void Development_uses_default_credential_chain()
    {
        var options = new AzurePlatformOptions(new Uri("https://example.blob.core.windows.net"), null);
        var credential = AzureCredentialFactory.Create(new TestEnvironment("Development"), options);
        Assert.IsType<DefaultAzureCredential>(credential);
    }

    private sealed class TestEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
