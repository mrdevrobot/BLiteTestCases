using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using LiteDB;

namespace PerformanceBenchmark;

public class WriteBenchmark
{
    private string _bliteDbPath = null!;
    private string _liteDbPath = null!;
    private List<BenchmarkPhotoPo> _photosToInsert = null!;

    [Params(20)] public int TotalFolders;

    [Params(5000)] public int TotalPhotos;

    [GlobalSetup]
    public void Setup()
    {
        var (photos, _, _) = DataGenerator.Generate(TotalPhotos, TotalFolders);
        _photosToInsert = photos;
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _liteDbPath = Path.Combine(Path.GetTempPath(), $"litedb_write_{Guid.NewGuid()}.db");
        _bliteDbPath = Path.Combine(Path.GetTempPath(), $"blitedb_write_{Guid.NewGuid()}");
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        if (File.Exists(_liteDbPath)) File.Delete(_liteDbPath);
        if (Directory.Exists(_bliteDbPath)) Directory.Delete(_bliteDbPath, true);
    }

    [Benchmark(Baseline = true)]
    public void LiteDB_BulkInsert()
    {
        using var db = new LiteDatabase($"Filename={_liteDbPath};Connection=Direct");
        var col = db.GetCollection<BenchmarkPhotoPo>("Photos");
        col.EnsureIndex(x => x.Id, true);
        col.EnsureIndex(x => x.SourceId);

        col.InsertBulk(_photosToInsert);
    }

    [Benchmark]
    public async Task BLite_BulkInsert()
    {
        using var db = new BenchmarkBLiteDbContext(_bliteDbPath);

        await db.Photos.InsertBulkAsync(_photosToInsert);
        await db.SaveChangesAsync();
    }
}