using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using Perfolizer.Horology;

namespace PerformanceBenchmark;

public class BenchmarkConfig
{
    public static IConfig Get()
    {
#if DEBUG
          return new DebugInProcessConfig()
           .AddJob(Job.Default.WithRuntime(CoreRuntime.Core10_0).WithPlatform(Platform.X64))
           .AddDiagnoser(MemoryDiagnoser.Default)
           .AddExporter(CsvExporter.Default)
           .AddExporter(HtmlExporter.Default)
           .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend).WithTimeUnit(Perfolizer.Horology.TimeUnit.Microsecond));
#else
        return DefaultConfig.Instance.AddJob(Job.Default.WithRuntime(CoreRuntime.Core10_0).WithPlatform(Platform.X64))
           .AddDiagnoser(MemoryDiagnoser.Default)
           .AddExporter(CsvExporter.Default)
           .AddExporter(HtmlExporter.Default)
           .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend).WithTimeUnit(TimeUnit.Microsecond));
#endif
    }
}