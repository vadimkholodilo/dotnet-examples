# Span<T> example: summing a CSV line

`SumCsvValues` sums the integers in a comma separated line such as `"12, 7, 105, -3"`.
Whitespace around values is ignored and empty values are skipped.

There are two implementations in [CsvExtensions.cs](CsvExtensions.cs) with identical behavior:

- **Without span**: `line.Split(',', ...)` and `int.Parse(string)` for each part.
  `Split` allocates an array plus one new `string` per value.
- **With span**: walks the original string with `ReadOnlySpan<char>`, using `IndexOf`, slicing and `Trim`,
  then calls `int.Parse(ReadOnlySpan<char>)`. Slices are just a pointer and a length into the
  original string, so nothing is copied or allocated.

## Results

Apple M4, macOS, .NET SDK 10.0.100, BenchmarkDotNet 0.15.8, default job, Release build.

| Values | Method      |          Mean | Ratio | Gen0 | Gen1 |  Allocated |
|-------:|-------------|--------------:|------:|-----:|-----:|-----------:|
|     10 | WithoutSpan |     132.84 ns |  1.00 |  0.0553 |    - |      464 B |
|     10 | WithSpan    |      71.90 ns |  0.54 |    - |    - |          0 |
|    100 | WithoutSpan |   1,309.01 ns |  1.00 |  0.5302 | 0.0076 |    4,448 B |
|    100 | WithSpan    |     716.66 ns |  0.55 |    - |    - |          0 |
| 10,000 | WithoutSpan | 134,487.48 ns |  1.00 | 52.0020 | 20.7520 | 436,216 B |
| 10,000 | WithSpan    |  84,932.98 ns |  0.63 |    - |    - |          0 |

## What the numbers say

- **Allocations drop to zero.** The classic version allocates roughly 44 bytes per value
  (436 KB for 10,000 values). The span version allocates nothing, so the garbage collector never has work to do.
- **It is also faster**: about 1.8x for short lines and about 1.6x for long ones. Most of the saved time is
  the cost of allocating and copying all those strings.
- **The gap narrows on long lines** because `int.Parse` itself becomes a bigger share of the time,
  and parsing costs the same in both versions.
- **GC pressure is the bigger win in practice.** In the baseline, a 10,000 value line triggers Gen1
  collections (20.75 per 1,000 calls). In a hot path such as a web request handler or a file import, this
  is what hurts throughput and tail latency, and a single microbenchmark timing does not show it.

## Run it yourself

```bash
dotnet run -c Release --project DotNetExamples.Span
```

Always use `-c Release`; Debug numbers are meaningless. Your absolute numbers will differ with hardware, but the
allocation column should match and the ratios should be similar.
