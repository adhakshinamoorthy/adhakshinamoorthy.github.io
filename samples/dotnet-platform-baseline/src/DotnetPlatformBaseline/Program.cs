using System.Text.Json;
using DotnetPlatformBaseline.Configuration;
using DotnetPlatformBaseline.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services
    .AddOptions<ProcessingOptions>()
    .Bind(builder.Configuration.GetSection(ProcessingOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.InputPath), "Processing:InputPath is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.OutputPath), "Processing:OutputPath is required.")
    .Validate(options => options.MaximumItems is > 0 and <= 10_000, "Processing:MaximumItems must be between 1 and 10000.")
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IManifestProcessor, ManifestProcessor>();

using var host = builder.Build();
var processor = host.Services.GetRequiredService<IManifestProcessor>();

try
{
    var report = await processor.ProcessAsync(CancellationToken.None);
    Console.WriteLine($"Processed {report.Items.Count} items for batch '{report.BatchId}'.");
    return 0;
}
catch (Exception exception) when (exception is IOException or JsonException or InvalidDataException)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
