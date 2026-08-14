using Azure.Core;
using Azure.Identity;

namespace AzureDotNetServiceBaseline;

public sealed record AzurePlatformOptions(Uri StorageEndpoint, string? ManagedIdentityClientId)
{
    public static AzurePlatformOptions FromConfiguration(IConfiguration configuration)
    {
        var endpoint = configuration["Azure:StorageEndpoint"];
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("Azure:StorageEndpoint must be an absolute HTTPS URI.");
        }

        return new(uri, configuration["Azure:ManagedIdentityClientId"]);
    }
}

public static class AzureCredentialFactory
{
    public static TokenCredential Create(IHostEnvironment environment, AzurePlatformOptions options) =>
        environment.IsDevelopment()
            ? new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeInteractiveBrowserCredential = true
            })
            : string.IsNullOrWhiteSpace(options.ManagedIdentityClientId)
                ? new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned)
                : new ManagedIdentityCredential(
                    ManagedIdentityId.FromUserAssignedClientId(options.ManagedIdentityClientId));
}
