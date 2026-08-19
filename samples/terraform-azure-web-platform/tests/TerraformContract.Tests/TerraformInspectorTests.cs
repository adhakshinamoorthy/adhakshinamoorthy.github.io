using TerraformContract;
using Xunit;

namespace TerraformContract.Tests;

public sealed class TerraformInspectorTests
{
    [Fact]
    public void Infrastructure_satisfies_the_platform_contract()
    {
        var result = TerraformInspector.Inspect(FindDirectory("infra"));
        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Checks.Where(check => !check.Passed).Select(check => check.Name)));
        Assert.Equal(7, result.Checks.Count);
    }

    [Fact]
    public void Inspector_rejects_an_incomplete_configuration()
    {
        var directory = Directory.CreateTempSubdirectory("terraform-contract-");
        try
        {
            File.WriteAllText(Path.Combine(directory.FullName, "main.tf"), "resource \"azurerm_resource_group\" \"main\" {}");
            Assert.False(TerraformInspector.Inspect(directory.FullName).IsValid);
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
