namespace DotnetPlatformBaseline.Configuration;

public sealed class ProcessingOptions
{
    public const string SectionName = "Processing";

    public string InputPath { get; init; } = "examples/work-items.json";

    public string OutputPath { get; init; } = "artifacts/work-report.json";

    public int MaximumItems { get; init; } = 1_000;
}
