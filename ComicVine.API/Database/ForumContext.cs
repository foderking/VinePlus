using ComicVine.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ComicVine.API.Database;

public class ForumContext: DbContext
{
    public DbSet<ForumThread> Threads { get; set; }
    public DbSet<ForumPost> Posts { get; set; }

    public ForumContext(DbContextOptions<ForumContext> options): base(options) {}
    
}