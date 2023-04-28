using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Comicvine.Core;
using Microsoft.Extensions.Configuration;

namespace Comicvine.Database;

public class ComicvineContext: DbContext
{
    public DbSet<Parsers.Thread> Threads { get; set; }
    public DbSet<Parsers.Post> Posts { get; set; }
    
    public ComicvineContext(DbContextOptions<ComicvineContext> options): base(options) {}
    

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
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