namespace BicepContract;

public sealed record ContractCheck(string Name, bool Passed);
public sealed record InspectionResult(IReadOnlyList<ContractCheck> Checks) { public bool IsValid => Checks.All(check => check.Passed); }

public static class BicepInspector
{
    public static InspectionResult Inspect(string directory)
    {
        if (!Directory.Exists(directory)) throw new DirectoryNotFoundException(directory);
        var source = string.Join('\n', Directory.GetFiles(directory, "*.bicep", SearchOption.AllDirectories).Order().Select(File.ReadAllText));
        return new(new[]
        {
            Check("declares resource-group scope", source.Contains("targetScope = 'resourceGroup'", StringComparison.Ordinal)),
            Check("uses modules and symbolic outputs", source.Contains("module observability", StringComparison.Ordinal) && source.Contains("observability.outputs", StringComparison.Ordinal)),
            Check("marks runtime input secure", source.Contains("@secure()", StringComparison.Ordinal) && source.Contains("secretRef", StringComparison.Ordinal)),
            Check("uses managed identity", source.Contains("SystemAssigned", StringComparison.Ordinal)),
            Check("enforces encrypted ingress", source.Contains("allowInsecure: false", StringComparison.Ordinal)),
            Check("defines readiness and liveness", source.Contains("type: 'Liveness'", StringComparison.Ordinal) && source.Contains("type: 'Readiness'", StringComparison.Ordinal)),
            Check("defines bounded scaling", source.Contains("minReplicas", StringComparison.Ordinal) && source.Contains("maxReplicas: 10", StringComparison.Ordinal)),
            Check("merges governance tags", source.Contains("union(tags, requiredTags)", StringComparison.Ordinal))
        });
    }

    private static ContractCheck Check(string name, bool passed) => new(name, passed);
}
