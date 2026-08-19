using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ReleaseManifest;

public sealed record BuildManifest(string Version, string CommitSha, string ArtifactSha256, string Runtime);

public static partial class ManifestFactory
{
    public static BuildManifest Create(string version, string commitSha, string artifactSha256)
    {
        if (!VersionPattern().IsMatch(version))
            throw new ArgumentException("Version must be a semantic version such as 1.4.2 or 1.4.2-rc.1.", nameof(version));
        if (!ShaPattern().IsMatch(commitSha) && !string.Equals(commitSha, "local", StringComparison.Ordinal))
            throw new ArgumentException("Commit SHA must contain 7 to 40 lowercase hexadecimal characters.", nameof(commitSha));
        if (!DigestPattern().IsMatch(artifactSha256))
            throw new ArgumentException("Artifact digest must be a 64-character lowercase SHA-256 value.", nameof(artifactSha256));

        return new(version, commitSha, artifactSha256, Environment.Version.ToString());
    }

    [GeneratedRegex(@"^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$", RegexOptions.CultureInvariant)]
    private static partial Regex VersionPattern();
    [GeneratedRegex("^[0-9a-f]{7,40}$", RegexOptions.CultureInvariant)]
    private static partial Regex ShaPattern();
    [GeneratedRegex("^[0-9a-f]{64}$", RegexOptions.CultureInvariant)]
    private static partial Regex DigestPattern();
}

[JsonSerializable(typeof(BuildManifest))]
public sealed partial class ReleaseJsonContext : JsonSerializerContext;
