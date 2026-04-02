using System;

namespace PerformanceBenchmark;

public class BenchmarkPhotoPo
{
    public Guid Id { get; init; }
    public string SourceId { get; init; } = null!;
    public string FilePath { get; init; } = null!;
    public DateTime DateTaken { get; set; }
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}