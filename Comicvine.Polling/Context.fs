module Comicvine.Polling.Context

open Comicvine.Core
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open System.Configuration

type ComicvineContext() =
  inherit DbContext()
  [<DefaultValue>] val mutable Threads: DbSet<Parsers.Thread>
  // [<DefaultValue>] val mutable Posts: DbSet<Parsers.Post>
  
  
  override _.OnConfiguring(optionsBuilder: DbContextOptionsBuilder) =
    let config = ConfigurationBuilder().AddJsonFile("settings.json", false, true).Build()
    optionsBuilder
      .UseNpgsql(config.GetConnectionString("comicvine_db"))
      .UseSnakeCaseNamingConvention()
    |> ignore
   
  override _.OnModelCreating(modelBuilder: ModelBuilder) =
    // modelBuilder.Entity<Parsers.Post>()
    //   .Property((fun x -> x.Creator))
    //   .HasColumnType("jsonb") |> ignore
      
    modelBuilder.Entity<Parsers.Thread>()
      .Property((fun x -> x.Board))
      .HasColumnType("jsonb") |> ignore
    modelBuilder.Entity<Parsers.Thread>()
      .Property((fun x -> x.Thread))
      .HasColumnType("jsonb") |> ignore
    modelBuilder.Entity<Parsers.Thread>()
      .Property((fun x -> x.Creator))
      .HasColumnType("jsonb") |> ignore
    
    // modelBuilder.Entity<Parsers.Thread>()
    //   .HasMany("Comments")
    //   .WithOne("")
    
    
    
    
    
  
  
  