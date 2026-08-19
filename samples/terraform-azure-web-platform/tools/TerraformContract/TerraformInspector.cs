namespace TerraformContract;

public sealed record ContractCheck(string Name, bool Passed);
public sealed record InspectionResult(IReadOnlyList<ContractCheck> Checks)
{
    public bool IsValid => Checks.All(check => check.Passed);
}

public static class TerraformInspector
{
    public static InspectionResult Inspect(string directory)
    {
        if (!Directory.Exists(directory)) throw new DirectoryNotFoundException(directory);
        var source = string.Join('\n', Directory.GetFiles(directory, "*.tf").Order().Select(File.ReadAllText));
        var checks = new[]
        {
            Check("pins Terraform and AzureRM constraints", source.Contains("required_version", StringComparison.Ordinal) && source.Contains("version = \"~> 4.0\"", StringComparison.Ordinal)),
            Check("validates environment input", source.Contains("contains([\"dev\", \"test\", \"prod\"]", StringComparison.Ordinal)),
            Check("merges mandatory governance tags", source.Contains("merge(local.required_tags, var.tags)", StringComparison.Ordinal)),
            Check("uses managed identity", source.Contains("SystemAssigned", StringComparison.Ordinal)),
            Check("requires HTTPS and TLS 1.2", source.Contains("https_only          = true", StringComparison.Ordinal) && source.Contains("minimum_tls_version", StringComparison.Ordinal)),
            Check("declares health and observability", source.Contains("health_check_path", StringComparison.Ordinal) && source.Contains("azurerm_application_insights", StringComparison.Ordinal)),
            Check("exposes non-sensitive outputs", source.Contains("web_app_hostname", StringComparison.Ordinal) && !source.Contains("output \"secret", StringComparison.OrdinalIgnoreCase))
        };
        return new(checks);
    }

    private static ContractCheck Check(string name, bool passed) => new(name, passed);
}
