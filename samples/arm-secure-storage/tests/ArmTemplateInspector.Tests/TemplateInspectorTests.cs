using ArmTemplateInspector;
using Xunit;

namespace ArmTemplateInspector.Tests;

public sealed class TemplateInspectorTests
{
    [Fact]
    public void Template_satisfies_the_secure_storage_contract()
    {
        var result = TemplateInspector.Inspect(FindFile("infra", "azuredeploy.json"));
        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Checks.Where(check => !check.Passed).Select(check => check.Name)));
        Assert.Equal(9, result.Checks.Count);
    }

    [Fact]
    public void Template_is_strict_json_and_has_three_resources()
    {
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(FindFile("infra", "azuredeploy.json")));
        Assert.Equal(3, document.RootElement.GetProperty("resources").GetArrayLength());
    }

    private static string FindFile(params string[] segments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine([current.FullName, .. segments]);
            if (File.Exists(candidate)) return candidate;
            current = current.Parent;
        }
        throw new FileNotFoundException(string.Join('/', segments));
    }
}
