using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Comicvine.Core;
using Microsoft.Extensions.Configuration;

namespace Comicvine.Database;

public class ComicvineContext: DbContext
{
    public DbSet<Parsers.Thread> Threads { get; set; }
    
    public ComicvineContext(DbContextOptions<ComicvineContext> options): base(options) {}
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("settings.json", false, true)
            .Build();
        optionsBuilder
            .UseNpgsql(config.GetConnectionString("comicvine_db"))
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Parsers.Thread>()
            .Property(x => x.Board)
            .HasColumnType("jsonb");
        modelBuilder.Entity<Parsers.Thread>()
            .Property(x => x.Thread)
            .HasColumnType("jsonb");
        modelBuilder.Entity<Parsers.Thread>()
            .Property(x => x.Creator)
            .HasColumnType("jsonb");
 
    }
}