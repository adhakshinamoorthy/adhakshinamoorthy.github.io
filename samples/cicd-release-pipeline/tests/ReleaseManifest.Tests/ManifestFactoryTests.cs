using ReleaseManifest;
using Xunit;

namespace ReleaseManifest.Tests;

public sealed class ManifestFactoryTests
{
    [Fact]
    public void Creates_traceable_release_metadata()
    {
        var digest = new string('a', 64);
        var manifest = ManifestFactory.Create("1.4.2", "abcdef1234567", digest);
        Assert.Equal("1.4.2", manifest.Version);
        Assert.Equal("abcdef1234567", manifest.CommitSha);
        Assert.Equal(digest, manifest.ArtifactSha256);
    }

    [Theory]
    [InlineData("latest")]
    [InlineData("1.2")]
    [InlineData("")]
    public void Rejects_non_semantic_versions(string version) =>
        Assert.Throws<ArgumentException>(() => ManifestFactory.Create(version, "abcdef1", new string('0', 64)));

    [Fact]
    public void Workflow_builds_once_and_promotes_the_artifact()
    {
        var workflow = File.ReadAllText(FindRepositoryFile(".github", "workflows", "release.yml"));
        Assert.Contains("needs: verify", workflow, StringComparison.Ordinal);
        Assert.Contains("actions/upload-artifact@v4", workflow, StringComparison.Ordinal);
        Assert.Contains("actions/download-artifact@v4", workflow, StringComparison.Ordinal);
        Assert.Contains("environment: production", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet build", workflow[(workflow.IndexOf("deploy:", StringComparison.Ordinal))..], StringComparison.Ordinal);
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        throw new FileNotFoundException(string.Join('/', segments));
    }
}
