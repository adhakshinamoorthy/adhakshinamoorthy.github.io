using System.Text.Json.Serialization;
using DotnetPlatformBaseline.Models;

namespace DotnetPlatformBaseline.Serialization;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(WorkManifest))]
[JsonSerializable(typeof(WorkReport))]
internal sealed partial class AppJsonContext : JsonSerializerContext;
