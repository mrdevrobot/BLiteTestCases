namespace NotEnoughSpaceIssue;

/// <summary>
/// https://github.com/EntglDb/BLite/issues/58 The smallest reproducible example of the issue
/// </summary>
internal class Program
{
    private static async Task Main()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"blite_fsi_repro_{Guid.NewGuid()}");
        if (Directory.Exists(dbPath)) Directory.Delete(dbPath, true);

        using var db = new BugReproDbContext(dbPath);

        // ====================================================================
        // PHASE 1: Fill page to near its 16KB physical limit.
        // - 2100 chars ≈ 2.1KB BSON per doc; 7 docs leave ~1KB physical free space.
        // ====================================================================
        var payload2100 = new string('A', 2100);
        var userIds = Enumerable.Range(0, 7).Select(_ => Guid.NewGuid()).ToList();

        Console.WriteLine("=> [1] Packing Page 1 to the limit...");
        foreach (var id in userIds)
        {
            await db.Users.InsertAsync(new User { Id = id, Name = payload2100 });
        }

        // ====================================================================
        // PHASE 2: Desync FSI (Free Space Inventory) via Delete + Rollback.
        // - Delete 2 docs: FSI incorrectly marks ~4.2KB as free;
        // - Rollback: Physical page reverts to ~1KB free, but FSI retains stale ~5KB free.
        // ====================================================================
        Console.WriteLine("=> [2] Poisoning FSI via Delete + Rollback...");
        using (var tx = await db.BeginTransactionAsync())
        {
            // Delete 2 docs to free ~4.2KB in FSI.
            await db.Users.DeleteAsync(userIds[0], tx);
            await db.Users.DeleteAsync(userIds[1], tx);

            // Rollback: Physical page reverts to ~1KB free, but FSI remains at ~5KB.
            await tx.RollbackAsync();
        }

        // ====================================================================
        // PHASE 3: Trigger inconsistency failure.
        // - Allocator trusts FSI (5KB > 2.1KB) → allows insert;
        // - InsertIntoPage checks physical space (1KB < 2.1KB) → throws fatal error.
        // - Expected: "need ~2100, have ~1000 | FSI=~5300" (proves stale FSI read).
        // ====================================================================
        Console.WriteLine("=> [3] Inserting new 2.1KB doc...");
        try
        {
            await db.Users.InsertAsync(new User { Id = Guid.NewGuid(), Name = payload2100 });
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=> SUCCESS: Bug not triggered.");
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[BINGO! BUG REPRODUCED SUCCESSFULLY!]");
            Console.WriteLine($"ErrorMessage: {ex.Message}");
        }
        finally
        {
            Console.ResetColor();
            if (Directory.Exists(dbPath)) Directory.Delete(dbPath, true);
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}