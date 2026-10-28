using System.Text;
using BenchmarkDotNet.Attributes;

namespace DotNetExamples.Span;

[MemoryDiagnoser]
public class CsvBenchmarks
{
    private string _line = string.Empty;

    [Params(10, 100, 10_000)]
    public int ValueCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        var builder = new StringBuilder();

        for (int i = 0; i < ValueCount; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(random.Next(-100_000, 100_000));
        }

        _line = builder.ToString();

        // Make sure both implementations agree before measuring anything.
        long expected = _line.SumCsvValuesWithoutSpan();
        long actual = _line.SumCsvValuesWithSpan();
        if (expected != actual)
        {
            throw new InvalidOperationException($"Implementations disagree: {expected} != {actual}");
        }
    }

    [Benchmark(Baseline = true)]
    public long WithoutSpan() => _line.SumCsvValuesWithoutSpan();

    [Benchmark]
    public long WithSpan() => _line.SumCsvValuesWithSpan();
}
