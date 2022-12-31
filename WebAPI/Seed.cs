using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebAPI.Database;
using WebAPI.Models;
using WebAPI.Scripts;

namespace WebAPI;

public static class Seed
{
    public static void Initialize(IServiceProvider serviceProvider, string? seedPath) {
        
        using ForumContext context = new ForumContext(
            serviceProvider.GetRequiredService<DbContextOptions<ForumContext>>()
        );
        if (context.Threads.Any()) return;

        if (File.Exists(seedPath)) {
            using FileStream file = File.OpenRead(seedPath);
            // Forum? forumThread = JsonSerializer.Deserialize<Forum>(File.ReadAllText(seedPath));
            Forum? forumThread = JsonSerializer.Deserialize<Forum>(file);

            if (forumThread == null) throw new Exception("Invalid Json file");
            var t = forumThread
                .threads
                .GroupBy(each => each.Id, each => each.ThreadTitle,
                    (k, g) => new { i = k, l = g.Count(), z = g.ToArray() })
                .Where(each => each.l > 1);

            context.Threads.AddRange(forumThread.threads);
            context.SaveChanges();
        }
        else throw new Exception("Invalid Seed Path");
    }
}