using System.Globalization;

namespace DotNetExamples.Span;

public readonly record struct SensorReading(int SensorId, long UnixSeconds, double Value);

public static class SensorReadingParser
{
    /// <summary>
    /// Parses a line such as "17,1793190896,21.5" into a <see cref="SensorReading"/>.
    /// Throws <see cref="FormatException"/> when the line does not have exactly three fields
    /// or a field is not valid.
    /// Classic implementation: every field becomes its own string.
    /// </summary>
    public static SensorReading ParseWithoutSpan(string line)
    {
        string[] parts = line.Split(',');
        if (parts.Length != 3)
        {
            throw new FormatException($"Expected 3 fields: '{line}'.");
        }

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int sensorId))
        {
            throw new FormatException($"Invalid sensor id: '{parts[0]}'.");
        }

        if (!long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long unixSeconds))
        {
            throw new FormatException($"Invalid timestamp: '{parts[1]}'.");
        }

        if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            throw new FormatException($"Invalid value: '{parts[2]}'.");
        }

        return new SensorReading(sensorId, unixSeconds, value);
    }

    /// <summary>
    /// Same behavior as <see cref="ParseWithoutSpan"/>, but the fields are slices of the original
    /// line, so nothing is allocated on the happy path.
    /// </summary>
    public static SensorReading ParseWithSpan(string line)
    {
        ReadOnlySpan<char> span = line;

        // One slot more than needed, so a line with too many fields is detected.
        Span<Range> fields = stackalloc Range[4];
        if (span.Split(fields, ',') != 3)
        {
            throw new FormatException($"Expected 3 fields: '{line}'.");
        }

        ReadOnlySpan<char> sensorIdPart = span[fields[0]];
        ReadOnlySpan<char> timestampPart = span[fields[1]];
        ReadOnlySpan<char> valuePart = span[fields[2]];

        if (!int.TryParse(sensorIdPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out int sensorId))
        {
            throw new FormatException($"Invalid sensor id: '{sensorIdPart}'.");
        }

        if (!long.TryParse(timestampPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out long unixSeconds))
        {
            throw new FormatException($"Invalid timestamp: '{timestampPart}'.");
        }

        if (!double.TryParse(valuePart, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            throw new FormatException($"Invalid value: '{valuePart}'.");
        }

        return new SensorReading(sensorId, unixSeconds, value);
    }
}
