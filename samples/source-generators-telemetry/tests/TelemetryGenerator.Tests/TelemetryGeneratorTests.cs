using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TelemetryDemo;
using Xunit;

namespace TelemetryGenerator.Tests;

public sealed class TelemetryGeneratorTests
{
    [Fact]
    public void Generator_EmitsTelemetryMethodForPublicProperties()
    {
        const string source = """
            using Telemetry.Generated;
            namespace Demo;

            [GenerateTelemetry]
            public partial record Payment(string Id, decimal Amount);
            """;

        var result = RunGenerator(source);
        var generated = Assert.Single(result.Results[0].GeneratedSources,
            source => source.HintName.EndsWith("Telemetry.g.cs", StringComparison.Ordinal));
        var text = generated.SourceText.ToString();

        Assert.Contains("partial record Payment", text, StringComparison.Ordinal);
        Assert.Contains("[\"Amount\"] = Amount", text, StringComparison.Ordinal);
        Assert.Contains("[\"Id\"] = Id", text, StringComparison.Ordinal);
        Assert.Empty(result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void Generator_ReportsDiagnosticForNonPartialType()
    {
        const string source = """
            using Telemetry.Generated;
            namespace Demo;

            [GenerateTelemetry]
            public sealed class Payment { public string Id { get; } = "42"; }
            """;

        var result = RunGenerator(source);

        var diagnostic = Assert.Single(result.Diagnostics, item => item.Id == "TSG001");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("must be partial", diagnostic.GetMessage(), StringComparison.Ordinal);
    }

    [Fact]
    public void Generator_ProducesStableOutputForEquivalentInput()
    {
        const string source = """
            using Telemetry.Generated;
            [GenerateTelemetry]
            public partial class Probe { public int Zeta { get; init; } public int Alpha { get; init; } }
            """;

        var first = GetTelemetrySource(RunGenerator(source));
        var second = GetTelemetrySource(RunGenerator(source));

        Assert.Equal(first, second);
        Assert.True(first.IndexOf("Alpha", StringComparison.Ordinal) < first.IndexOf("Zeta", StringComparison.Ordinal));
    }

    [Fact]
    public void Generator_ReportsDiagnosticForUnsupportedGenericType()
    {
        const string source = """
            using Telemetry.Generated;
            [GenerateTelemetry]
            public partial class Envelope<T> { public T? Value { get; init; } }
            """;

        var result = RunGenerator(source);

        var diagnostic = Assert.Single(result.Diagnostics, item => item.Id == "TSG002");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("top-level, non-generic", diagnostic.GetMessage(), StringComparison.Ordinal);
    }

    [Fact]
    public void Consumer_UsesGeneratedMethodAtRuntime()
    {
        var order = new OrderCompleted(
            Guid.Parse("10000000-0000-0000-0000-000000000099"),
            "customer-7",
            29.95m,
            DateTimeOffset.UnixEpoch);

        var telemetry = order.ToTelemetry();

        Assert.Equal("customer-7", telemetry[nameof(order.CustomerId)]);
        Assert.Equal(29.95m, telemetry[nameof(order.Total)]);
        Assert.Equal(4, telemetry.Count);
    }

    private static GeneratorDriverRunResult RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14));
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create(
            "GeneratorTests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new global::TelemetryGenerator.TelemetryGenerator().AsSourceGenerator()],
            parseOptions: (CSharpParseOptions)syntaxTree.Options);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        return driver.GetRunResult();
    }

    private static string GetTelemetrySource(GeneratorDriverRunResult result) =>
        Assert.Single(result.Results[0].GeneratedSources,
            source => source.HintName.EndsWith("Telemetry.g.cs", StringComparison.Ordinal))
            .SourceText.ToString();
}
