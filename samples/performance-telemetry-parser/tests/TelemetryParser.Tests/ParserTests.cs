using System.Globalization;
using Xunit;

namespace TelemetryParser.Tests;

public sealed class ParserTests
{
    private const string Valid = "42|638907696000000000|73.25|1";

    [Fact]
    public void Implementations_have_identical_result()
    {
        Assert.True(TelemetryLineParser.TryParseBaseline(Valid, out var baseline));
        Assert.True(TelemetryLineParser.TryParseSpan(Valid, out var optimized));
        Assert.Equal(baseline, optimized);
    }

    [Theory]
    [InlineData("")]
    [InlineData("42|1|2")]
    [InlineData("42|ticks|73.25|1")]
    [InlineData("42|1|73.25|1|extra")]
    public void Invalid_input_is_rejected(string line) => Assert.False(TelemetryLineParser.TryParseSpan(line, out _));

    [Fact]
    public void Parsing_is_culture_independent()
    {
        var original = CultureInfo.CurrentCulture;
        try { CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR"); Assert.True(TelemetryLineParser.TryParseSpan(Valid, out var result)); Assert.Equal(73.25, result.Value); }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Fact]
    public void Span_path_allocates_less_than_split_path()
    {
        TelemetryLineParser.TryParseBaseline(Valid, out _);
        TelemetryLineParser.TryParseSpan(Valid, out _);
        var beforeBaseline = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 100; index++) TelemetryLineParser.TryParseBaseline(Valid, out _);
        var baselineBytes = GC.GetAllocatedBytesForCurrentThread() - beforeBaseline;
        var beforeSpan = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 100; index++) TelemetryLineParser.TryParseSpan(Valid, out _);
        var spanBytes = GC.GetAllocatedBytesForCurrentThread() - beforeSpan;
        Assert.True(spanBytes < baselineBytes, $"Expected span allocation {spanBytes} to be lower than baseline {baselineBytes}.");
    }
}
