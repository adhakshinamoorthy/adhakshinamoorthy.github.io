using DotnetPlatformBaseline.Models;

namespace DotnetPlatformBaseline.Services;

public interface IManifestProcessor
{
    Task<WorkReport> ProcessAsync(CancellationToken cancellationToken);
}
