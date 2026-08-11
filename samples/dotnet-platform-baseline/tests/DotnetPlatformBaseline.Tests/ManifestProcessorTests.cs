using DotnetPlatformBaseline.Configuration;
using DotnetPlatformBaseline.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DotnetPlatformBaseline.Tests;

public sealed class ManifestProcessorTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"dotnet-platform-{Guid.NewGuid():N}");

    public ManifestProcessorTests() => Directory.CreateDirectory(_directory);

    [Fact]
    public async Task ProcessAsync_WritesDeterministicReport()
    {
        var input = await WriteManifestAsync("""
            {"batchId":"batch-1","items":[{"id":"one","payload":"hello"}]}
            """);
        var output = Path.Combine(_directory, "out", "report.json");
        var processor = CreateProcessor(input, output);

        var report = await processor.ProcessAsync(CancellationToken.None);

        Assert.Equal("batch-1", report.BatchId);
        Assert.Equal(new DateTimeOffset(2026, 8, 11, 0, 0, 0, TimeSpan.Zero), report.ProcessedAtUtc);
        Assert.Equal(5, report.Items[0].PayloadBytes);
        Assert.Equal("2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824", report.Items[0].Sha256);
        Assert.True(File.Exists(output));
    }

    [Fact]
    public async Task ProcessAsync_RejectsDuplicateIdentifiers()
    {
        var input = await WriteManifestAsync("""
            {"batchId":"batch-1","items":[{"id":"one","payload":"a"},{"id":"ONE","payload":"b"}]}
            """);
        var processor = CreateProcessor(input, Path.Combine(_directory, "report.json"));

        var error = await Assert.ThrowsAsync<InvalidDataException>(
            () => processor.ProcessAsync(CancellationToken.None));

        Assert.Contains("unique", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProcessAsync_EnforcesConfiguredItemLimit()
    {
        var input = await WriteManifestAsync("""
            {"batchId":"batch-1","items":[{"id":"one","payload":"a"},{"id":"two","payload":"b"}]}
            """);
        var processor = CreateProcessor(input, Path.Combine(_directory, "report.json"), maximumItems: 1);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => processor.ProcessAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ProcessAsync_ReportsMissingInput()
    {
        var processor = CreateProcessor(
            Path.Combine(_directory, "missing.json"),
            Path.Combine(_directory, "report.json"));

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => processor.ProcessAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ProcessAsync_HonorsCancellation()
    {
        var input = await WriteManifestAsync("""
            {"batchId":"batch-1","items":[{"id":"one","payload":"a"}]}
            """);
        var processor = CreateProcessor(input, Path.Combine(_directory, "report.json"));
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => processor.ProcessAsync(cancellation.Token));
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private async Task<string> WriteManifestAsync(string json)
    {
        var path = Path.Combine(_directory, $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, json, CancellationToken.None);
        return path;
    }

    private static ManifestProcessor CreateProcessor(string input, string output, int maximumItems = 1000)
    {
        var options = Options.Create(new ProcessingOptions
        {
            InputPath = input,
            OutputPath = output,
            MaximumItems = maximumItems
        });

        return new ManifestProcessor(
            options,
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 11, 0, 0, 0, TimeSpan.Zero)),
            NullLogger<ManifestProcessor>.Instance);
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
