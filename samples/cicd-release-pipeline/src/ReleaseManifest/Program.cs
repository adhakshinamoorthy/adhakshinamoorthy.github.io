using System.Text.Json;
using ReleaseManifest;

try
{
    var manifest = ManifestFactory.Create(
        Environment.GetEnvironmentVariable("RELEASE_VERSION") ?? "0.0.0-local",
        Environment.GetEnvironmentVariable("GIT_SHA") ?? "local",
        Environment.GetEnvironmentVariable("ARTIFACT_SHA256") ?? new string('0', 64));
    Console.WriteLine(JsonSerializer.Serialize(manifest, ReleaseJsonContext.Default.BuildManifest));
    return 0;
}
catch (ArgumentException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
