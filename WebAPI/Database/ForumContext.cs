using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Database;

public class ForumContext: DbContext
{
    public DbSet<ForumThread> Threads { get; set; }

    public ForumContext(DbContextOptions<ForumContext> options): base(options) {}
    
}