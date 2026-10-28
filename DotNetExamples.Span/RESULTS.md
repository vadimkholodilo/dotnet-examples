# Span<T> example: parsing a sensor reading line

`SensorReadingParser` turns a CSV line such as `"17,1793190896,21.5"` into a
`SensorReading(int SensorId, long UnixSeconds, double Value)`.
It throws `FormatException` when the line does not have exactly three fields or a field is invalid.

The result is a struct of numbers, not strings. That is what makes this a fair test: the fields of
the line are only needed long enough to be parsed, so any string created for them is pure garbage.

There are two implementations in [SensorReadingParser.cs](SensorReadingParser.cs) with identical behavior:

- **Without span**: `line.Split(',')` and `TryParse(string)` for each field.
  `Split` allocates an array plus one new `string` per field.
- **With span**: `ReadOnlySpan<char>.Split` writes the field boundaries into a `stackalloc`ed
  `Span<Range>`, each field is sliced from the original line and parsed with `TryParse(ReadOnlySpan<char>)`.
  Slices are just a pointer and a length into the original string, so nothing is copied or allocated.

The benchmark parses a whole batch of lines (think of reading a file) and sums the values.

## Results

Apple M4, macOS, .NET SDK 10.0.100, BenchmarkDotNet 0.15.8, default job, Release build.

| Lines  | Method      |       Mean | Ratio |     Gen0 |   Allocated |
|-------:|-------------|-----------:|------:|---------:|------------:|
|    100 | WithoutSpan |   5.525 μs |  1.00 |   1.9073 |    15,976 B |
|    100 | WithSpan    |   4.774 μs |  0.86 |        - |           0 |
| 10,000 | WithoutSpan | 562.370 μs |  1.00 | 190.4297 | 1,598,504 B |
| 10,000 | WithSpan    | 493.995 μs |  0.88 |        - |           0 |

## What the numbers say

- **Allocations drop to zero.** The classic version allocates about 160 bytes per line (an array and three
  strings). The span version allocates nothing, so the garbage collector never has work to do.
- **It is also faster, but modestly**: about 12 to 14%. Parsing the numbers costs the same in both versions,
  so only the allocating and copying is saved.
- **GC pressure is the bigger win in practice.** Importing 10,000 lines creates 1.6 MB of short-lived garbage
  in the baseline (190 Gen0 collections per 1,000 calls). In a hot path such as a file import or a message
  consumer, this is what hurts throughput and tail latency.

## Why not return strings?

A parser that returns `string[]` or `List<string>` needs the strings anyway, so `Span` cannot save anything.
It pays off when the strings are only an intermediate step towards typed values.

## Run it yourself

```bash
dotnet run -c Release --project DotNetExamples.Span
```

Always use `-c Release`; Debug numbers are meaningless. Your absolute numbers will differ with hardware, but the
allocation column should match and the ratios should be similar.
