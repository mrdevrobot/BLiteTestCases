using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;

namespace PerformanceBenchmark;

public static class DataGenerator
{
    public static (List<BenchmarkPhotoPo> Photos, List<string> SampleSourceIds, List<string> SampleFilePaths) Generate(int totalPhotos, int totalFolders)
    {
        Console.WriteLine("=> Start generating test data with Bogus...");

        var folderIds = Enumerable.Range(1, totalFolders)
           .Select(_ => Guid.CreateVersion7().ToString("N")[..8]) // Simulated 8-bit NanoID
           .ToList();

        var photoFaker = new Faker<BenchmarkPhotoPo>().RuleFor(x => x.Id, f => Guid.CreateVersion7())
           .RuleFor(x => x.SourceId, f => f.PickRandom(folderIds))
           .RuleFor(x => x.FilePath, (f, u) => $"D:\\Photos\\{u.SourceId}\\IMG_{f.Random.AlphaNumeric(8)}.jpg")
           .RuleFor(x => x.DateTaken, f => f.Date.Past(10))
           .RuleFor(x => x.FileSize, f => f.Random.Long(1024 * 1024, 1024 * 1024 * 15)) // 1MB - 15MB
           .RuleFor(x => x.Width, f => f.PickRandom(1920, 2560, 3840, 4000))
           .RuleFor(x => x.Height, f => f.PickRandom(1080, 1440, 2160, 3000));

        var photos = photoFaker.Generate(totalPhotos);

        var sampleSourceIds = folderIds.OrderBy(x => Guid.NewGuid()).Take(2).ToList();
        var sampleFilePaths = photos.OrderBy(x => Guid.NewGuid()).Select(x => x.FilePath).Take(100).ToList();

        Console.WriteLine("=> Generation Complete: {photos.Count} Photos, Distributed Across {folderIds.Count} Folders.");
        return (photos, sampleSourceIds, sampleFilePaths);
    }
}