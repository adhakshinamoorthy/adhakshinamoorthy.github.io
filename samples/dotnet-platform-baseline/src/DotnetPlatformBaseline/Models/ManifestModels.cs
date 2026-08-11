namespace DotnetPlatformBaseline.Models;

public sealed record WorkManifest(string BatchId, IReadOnlyList<WorkItem> Items);

public sealed record WorkItem(string Id, string Payload);

public sealed record WorkItemResult(string Id, int PayloadBytes, string Sha256);

public sealed record PlatformSnapshot(
    string Framework,
    string OperatingSystem,
    string ProcessArchitecture,
    string RuntimeIdentifier);

public sealed record WorkReport(
    string BatchId,
    DateTimeOffset ProcessedAtUtc,
    PlatformSnapshot Platform,
    IReadOnlyList<WorkItemResult> Items);
