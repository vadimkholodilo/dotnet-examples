# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Purpose

Small, self-contained .NET examples that accompany the author's blog posts. Code is written to be copied into a post, so each example should stay readable on its own and avoid helpers or shared infrastructure that would have to be carried along (e.g. exception helpers are inlined at the throw site rather than extracted).

## Commands

Target is .NET 10 (`net10.0`), nullable and implicit usings enabled.

```bash
dotnet build
dotnet run -c Release --project DotNetExamples.Span                       # runs all benchmarks
dotnet run -c Release --project DotNetExamples.Span -- --filter "*CsvBenchmarks*"   # filter via BenchmarkDotNet args
```

Always run benchmarks in `Release`; Debug numbers are meaningless. There is no test project or linter.

## Structure

- `dotnet-examples.sln` holds one project per example (currently only `DotNetExamples.Span`).
- `Directory.Build.props` adds the `BenchmarkDotNet` package reference to every project, so new example projects get it without declaring it.
- Each example project is an `Exe` whose `Program.cs` is a `BenchmarkSwitcher` entry point that forwards CLI args.
- In `DotNetExamples.Span`, `CsvExtensions.cs` has two implementations of the same behavior (`...WithoutSpan` vs `...WithSpan`), which `CsvBenchmarks.cs` compares. Its `[GlobalSetup]` asserts both return the same result before measuring, so keep the two implementations behaviorally identical (including the `FormatException` message and index).
- `RESULTS.md` in each example records the benchmark output and explanation used for the post; update it when the implementations or benchmarks change.

## Workflow

Solo repository: commit directly to `master` and push; do not create feature branches or PRs. `.idea/` is untracked and should not be committed.
