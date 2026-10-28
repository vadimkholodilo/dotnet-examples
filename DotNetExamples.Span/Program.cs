using BenchmarkDotNet.Running;
using DotNetExamples.Span;

// Run in Release, otherwise the numbers are meaningless:
//   dotnet run -c Release --project DotNetExamples.Span
// Pass BenchmarkDotNet args to filter, e.g. -- --filter "*SensorReadingBenchmarks*"
BenchmarkSwitcher.FromAssembly(typeof(SensorReadingBenchmarks).Assembly).Run(args);
