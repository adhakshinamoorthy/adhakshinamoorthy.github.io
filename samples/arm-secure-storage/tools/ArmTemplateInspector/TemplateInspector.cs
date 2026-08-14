using System.Text.Json;

namespace ArmTemplateInspector;

public sealed record ContractCheck(string Name, bool Passed);
public sealed record InspectionResult(IReadOnlyList<ContractCheck> Checks) { public bool IsValid => Checks.All(check => check.Passed); }

public static class TemplateInspector
{
    public static InspectionResult Inspect(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path), new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Disallow });
        var root = document.RootElement;
        var resources = root.GetProperty("resources").EnumerateArray().ToArray();
        var storage = resources.Single(resource => resource.GetProperty("type").GetString() == "Microsoft.Storage/storageAccounts");
        var properties = storage.GetProperty("properties");
        var source = File.ReadAllText(path);

        return new(new[]
        {
            Check("uses the ARM deployment schema", root.GetProperty("$schema").GetString()?.Contains("deploymentTemplate.json", StringComparison.Ordinal) == true),
            Check("constrains environment values", root.GetProperty("parameters").GetProperty("environment").GetProperty("allowedValues").GetArrayLength() == 3),
            Check("uses deterministic unique naming", source.Contains("uniqueString(resourceGroup().id)", StringComparison.Ordinal)),
            Check("merges required governance tags", source.Contains("union(parameters('tags'), variables('requiredTags'))", StringComparison.Ordinal)),
            Check("requires HTTPS and TLS 1.2", properties.GetProperty("supportsHttpsTrafficOnly").GetBoolean() && properties.GetProperty("minimumTlsVersion").GetString() == "TLS1_2"),
            Check("disables public blobs and shared keys", !properties.GetProperty("allowBlobPublicAccess").GetBoolean() && !properties.GetProperty("allowSharedKeyAccess").GetBoolean()),
            Check("defaults network access to deny", properties.GetProperty("networkAcls").GetProperty("defaultAction").GetString() == "Deny"),
            Check("enables recovery and lifecycle controls", source.Contains("deleteRetentionPolicy", StringComparison.Ordinal) && source.Contains("managementPolicies", StringComparison.Ordinal)),
            Check("outputs identifiers without credentials", root.GetProperty("outputs").EnumerateObject().All(output => !output.Name.Contains("key", StringComparison.OrdinalIgnoreCase) && !output.Name.Contains("secret", StringComparison.OrdinalIgnoreCase)))
        });
    }

    private static ContractCheck Check(string name, bool passed) => new(name, passed);
}
