using BicepContract;
using Xunit;

namespace BicepContract.Tests;

public sealed class BicepInspectorTests
{
    [Fact]
    public void Modules_satisfy_the_container_platform_contract()
    {
        var result = BicepInspector.Inspect(FindDirectory("infra"));
        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Checks.Where(check => !check.Passed).Select(check => check.Name)));
        Assert.Equal(8, result.Checks.Count);
    }

    [Fact]
    public void Inspector_rejects_an_incomplete_template()
    {
        var directory = Directory.CreateTempSubdirectory("bicep-contract-");
        try
        {
            File.WriteAllText(Path.Combine(directory.FullName, "main.bicep"), "resource group 'Microsoft.Resources/resourceGroups@2024-03-01' = {}");
            Assert.False(BicepInspector.Inspect(directory.FullName).IsValid);
        }
        finally { directory.Delete(true); }
    }

    private static string FindDirectory(string name)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, name);
            if (Directory.Exists(candidate)) return candidate;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException(name);
    }
}
