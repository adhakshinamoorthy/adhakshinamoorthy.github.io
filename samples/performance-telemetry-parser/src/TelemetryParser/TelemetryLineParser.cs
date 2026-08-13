using System.Globalization;

namespace TelemetryParser;

public readonly record struct TelemetryReading(int DeviceId, long TimestampTicks, double Value, int Status);

public static class TelemetryLineParser
{
    public static bool TryParseBaseline(string line, out TelemetryReading reading)
    {
        var fields = line.Split('|');
        if (fields.Length == 4 &&
            int.TryParse(fields[0], NumberStyles.None, CultureInfo.InvariantCulture, out var deviceId) &&
            long.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out var timestamp) &&
            double.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var value) &&
            int.TryParse(fields[3], NumberStyles.None, CultureInfo.InvariantCulture, out var status))
        {
            reading = new(deviceId, timestamp, value, status);
            return true;
        }
        reading = default;
        return false;
    }

    public static bool TryParseSpan(ReadOnlySpan<char> line, out TelemetryReading reading)
    {
        Span<Range> fields = stackalloc Range[4];
        var fieldCount = 0;
        var start = 0;
        for (var index = 0; index <= line.Length; index++)
        {
            if (index != line.Length && line[index] != '|') continue;
            if (fieldCount == fields.Length) { reading = default; return false; }
            fields[fieldCount++] = start..index;
            start = index + 1;
        }
        if (fieldCount == 4 &&
            int.TryParse(line[fields[0]], NumberStyles.None, CultureInfo.InvariantCulture, out var deviceId) &&
            long.TryParse(line[fields[1]], NumberStyles.None, CultureInfo.InvariantCulture, out var timestamp) &&
            double.TryParse(line[fields[2]], NumberStyles.Float, CultureInfo.InvariantCulture, out var value) &&
            int.TryParse(line[fields[3]], NumberStyles.None, CultureInfo.InvariantCulture, out var status))
        {
            reading = new(deviceId, timestamp, value, status);
            return true;
        }
        reading = default;
        return false;
    }
}
