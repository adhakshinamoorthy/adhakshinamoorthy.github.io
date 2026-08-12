using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotnetPlatformBaseline.Configuration;
using DotnetPlatformBaseline.Models;
using DotnetPlatformBaseline.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetPlatformBaseline.Services;

public sealed class ManifestProcessor(
    IOptions<ProcessingOptions> options,
    TimeProvider timeProvider,
    ILogger<ManifestProcessor> logger) : IManifestProcessor
{
    private readonly ProcessingOptions _options = options.Value;

    public async Task<WorkReport> ProcessAsync(CancellationToken cancellationToken)
    {
        var inputPath = Path.IsPathRooted(_options.InputPath)
            ? Path.GetFullPath(_options.InputPath)
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _options.InputPath));
        var outputPath = Path.GetFullPath(_options.OutputPath);

        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("The configured work manifest does not exist.", inputPath);
        }

        await using var input = File.OpenRead(inputPath);
        var manifest = await JsonSerializer.DeserializeAsync(
            input,
            AppJsonContext.Default.WorkManifest,
            cancellationToken) ?? throw new InvalidDataException("The work manifest is empty.");

        Validate(manifest);
        var results = manifest.Items.Select(item => CreateResult(item, cancellationToken)).ToArray();
        var report = new WorkReport(
            manifest.BatchId,
            timeProvider.GetUtcNow(),
            new PlatformSnapshot(
                RuntimeInformation.FrameworkDescription,
                RuntimeInformation.OSDescription,
                RuntimeInformation.ProcessArchitecture.ToString(),
                RuntimeInformation.RuntimeIdentifier),
            results);

        var outputDirectory = Path.GetDirectoryName(outputPath)
            ?? throw new InvalidOperationException("The output path must include a directory.");
        Directory.CreateDirectory(outputDirectory);

        var temporaryPath = outputPath + ".tmp";
        await using (var output = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(
                output,
                report,
                AppJsonContext.Default.WorkReport,
                cancellationToken);
        }

        File.Move(temporaryPath, outputPath, overwrite: true);
        logger.LogInformation(
            "Processed batch {BatchId} with {ItemCount} items into {OutputPath}",
            report.BatchId,
            report.Items.Count,
            outputPath);

        return report;
    }

    private static WorkItemResult CreateResult(WorkItem item, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var payload = Encoding.UTF8.GetBytes(item.Payload);
        return new WorkItemResult(item.Id, payload.Length, Convert.ToHexString(SHA256.HashData(payload)));
    }

    private void Validate(WorkManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.BatchId))
        {
            throw new InvalidDataException("batchId is required.");
        }

        if (manifest.Items.Count is 0 || manifest.Items.Count > _options.MaximumItems)
        {
            throw new InvalidDataException($"items must contain between 1 and {_options.MaximumItems} entries.");
        }

        if (manifest.Items.Any(item => string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.Payload)))
        {
            throw new InvalidDataException("Every item requires a non-empty id and payload.");
        }

        if (manifest.Items.Select(item => item.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != manifest.Items.Count)
        {
            throw new InvalidDataException("Item ids must be unique within a batch.");
        }
    }
}
