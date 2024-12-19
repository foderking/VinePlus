using Microsoft.EntityFrameworkCore;
using Comicvine.Core;

namespace VinePlus.Database;

public class ComicvineContext: DbContext
{
    public DbSet<Parsers.Thread> Threads { get; set; } = null!;
    public DbSet<Parsers.Post> Posts { get; set; } = null!;

    public ComicvineContext(DbContextOptions<ComicvineContext> options): base(options) {}
    

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // index
        //modelBuilder.Entity<Parsers.Post>()
        //    .HasIndex(post => post.Created)
        //    .IsDescending(true);
        modelBuilder.Entity<Parsers.Thread>()
            .HasIndex(thread => thread.Created)
            .IsDescending(true);
        // Link type should be serialized as json
        modelBuilder.Entity<Parsers.Thread>()
            .Property(x => x.Board)
            .HasColumnType("jsonb");
        modelBuilder.Entity<Parsers.Thread>()
            .Property(x => x.Thread)
            .HasColumnType("jsonb");
        modelBuilder.Entity<Parsers.Thread>()
            .Property(x => x.Creator)
            .HasColumnType("jsonb");
        modelBuilder.Entity<Parsers.Post>()
            .Property(x => x.Creator)
            .HasColumnType("jsonb");
    }
}