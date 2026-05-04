using Microsoft.EntityFrameworkCore;
using EchoVault.Core.Entities;

namespace EchoVault.Infrastructure.Data;

public class VaultDbContext : DbContext
{
  public VaultDbContext(DbContextOptions<VaultDbContext> options) : base(options) { }

  public DbSet<User> Users => Set<User>();
  public DbSet<WatchFolder> WatchFolders => Set<WatchFolder>();
  public DbSet<Document> Documents => Set<Document>();
  public DbSet<DocumentChunk> Chunks => Set<DocumentChunk>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // User Configuration
    modelBuilder.Entity<User>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.HasIndex(e => e.Username).IsUnique();
    });

    // Document Configuration
    modelBuilder.Entity<Document>()
        .HasOne(d => d.WatchFolder)
        .WithMany(w => w.Documents)
        .HasForeignKey(d => d.WatchFolderId);

    // Vector Configuration (The "Niche" Part)
    modelBuilder.Entity<DocumentChunk>(entity =>
    {
      entity.HasOne(c => c.Document)
          .WithMany(d => d.Chunks)
          .HasForeignKey(c => c.DocumentId);

      // We store the float array as a BLOB. 
      // sqlite-vec interprets BLOBs as vector data.
      entity.Property(e => e.Embedding)
          .HasColumnType("BLOB");
    });
  }
}