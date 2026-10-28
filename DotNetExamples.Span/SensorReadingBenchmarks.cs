using System.Globalization;
using BenchmarkDotNet.Attributes;

namespace DotNetExamples.Span;

[MemoryDiagnoser]
public class SensorReadingBenchmarks
{
    private string[] _lines = [];

    [Params(100, 10_000)]
    public int LineCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        long start = 1_793_190_896;
        _lines = new string[LineCount];

        for (int i = 0; i < LineCount; i++)
        {
            int sensorId = random.Next(1, 500);
            long unixSeconds = start + i;
            double value = Math.Round(random.NextDouble() * 100, 2);

            _lines[i] = string.Create(CultureInfo.InvariantCulture, $"{sensorId},{unixSeconds},{value}");
        }

        // Make sure both implementations agree before measuring anything.
        foreach (string line in _lines)
        {
            SensorReading expected = SensorReadingParser.ParseWithoutSpan(line);
            SensorReading actual = SensorReadingParser.ParseWithSpan(line);
            if (expected != actual)
            {
                throw new InvalidOperationException($"Implementations disagree on '{line}': {expected} != {actual}");
            }
        }
    }

    [Benchmark(Baseline = true)]
    public double WithoutSpan()
    {
        double sum = 0;
        foreach (string line in _lines)
        {
            sum += SensorReadingParser.ParseWithoutSpan(line).Value;
        }

        return sum;
    }

    [Benchmark]
    public double WithSpan()
    {
        double sum = 0;
        foreach (string line in _lines)
        {
            sum += SensorReadingParser.ParseWithSpan(line).Value;
        }

        return sum;
    }
}
