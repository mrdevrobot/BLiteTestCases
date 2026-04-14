using BLite.Core;
using BLite.Core.Collections;
using BLite.Core.Metadata;

namespace NotEnoughSpaceIssue;

public partial class BugReproDbContext : DocumentDbContext
{
    public DocumentCollection<Guid, User> Users { get; set; } = null!;

    public BugReproDbContext(string path) : base(path)
    {
        InitializeCollections();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToCollection("Users").HasIndex(x => x.Id, unique: true);
    }
}