using System.Globalization;

namespace DotNetExamples.Span;

public static class CsvExtensions
{
    /// <summary>
    /// Sums the integers in a comma separated line such as "12, 7, 105,-3".
    /// Whitespace around values is ignored and empty values are skipped.
    /// Throws <see cref="FormatException"/> with the zero-based index of the value
    /// (counting empty ones) when it is not a valid <see cref="int"/>.
    /// Classic implementation: every value becomes its own string.
    /// </summary>
    public static long SumCsvValuesWithoutSpan(this string line)
    {
        long sum = 0;

        string[] parts = line.Split(',', StringSplitOptions.TrimEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length == 0)
            {
                continue;
            }

            if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                throw new FormatException($"Value at index {i} is not a valid integer: '{parts[i]}'.");
            }

            sum += value;
        }

        return sum;
    }

    /// <summary>
    /// Same behavior as <see cref="SumCsvValuesWithoutSpan"/>, but it only slices the original
    /// string with <see cref="ReadOnlySpan{T}"/>, so nothing is allocated on the happy path.
    /// </summary>
    public static long SumCsvValuesWithSpan(this string line)
    {
        long sum = 0;
        ReadOnlySpan<char> remaining = line;
        int index = -1;

        while (!remaining.IsEmpty)
        {
            index++;
            int comma = remaining.IndexOf(',');
            ReadOnlySpan<char> part;

            if (comma < 0)
            {
                part = remaining;
                remaining = default;
            }
            else
            {
                part = remaining[..comma];
                remaining = remaining[(comma + 1)..];
            }

            part = part.Trim();
            if (part.IsEmpty)
            {
                continue;
            }

            if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                throw new FormatException($"Value at index {index} is not a valid integer: '{part}'.");
            }

            sum += value;
        }

        return sum;
    }
}
