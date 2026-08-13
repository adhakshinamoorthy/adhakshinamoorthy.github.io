using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TelemetryParser;

BenchmarkRunner.Run<ParserBenchmarks>(args: args);

[MemoryDiagnoser]
[ShortRunJob]
public class ParserBenchmarks
{
    private const string Line = "42|638907696000000000|73.25|1";

    [Benchmark(Baseline = true)]
    public TelemetryReading Split()
    {
        TelemetryLineParser.TryParseBaseline(Line, out var reading);
        return reading;
    }

    [Benchmark]
    public TelemetryReading Span()
    {
        TelemetryLineParser.TryParseSpan(Line, out var reading);
        return reading;
    }
}
