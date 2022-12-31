using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebAPI.Database;
using WebAPI.Models;
using WebAPI.Scripts;

namespace WebAPI;
using Repository;

public static class Seed
{
    public static void InitializeForums(IServiceProvider serviceProvider, string? seedPath) {
        
        using ForumContext context = new ForumContext(
            serviceProvider.GetRequiredService<DbContextOptions<ForumContext>>()
        );
        if (context.Threads.Any()) return;

        if (File.Exists(seedPath)) {
            using FileStream file = File.OpenRead(seedPath);
            // Forum? forumThread = JsonSerializer.Deserialize<Forum>(File.ReadAllText(seedPath));
            Forum? forumThread = JsonSerializer.Deserialize<Forum>(file);

            if (forumThread == null) throw new Exception("Invalid Json file");
            // var t = forumThread
            //     .threads
            //     .GroupBy(each => each.Id, each => each.ThreadTitle,
            //         (k, g) => new { i = k, l = g.Count(), z = g.ToArray() })
            //     .Where(each => each.l > 1);

            context.Threads.AddRange(forumThread.threads);
            context.SaveChanges();
        }
        else throw new Exception("Invalid Seed Path");
    }
    public static async Task InitializePosts(IServiceProvider serviceProvider) {
        await using ForumContext context = new ForumContext(
            serviceProvider.GetRequiredService<DbContextOptions<ForumContext>>()
        );
        
        PostRepository postRepo = new ();
        if (!context.Threads.Any()) return;
        

        await Parallel.ForEachAsync(context.Threads, new ParallelOptions {MaxDegreeOfParallelism = 10 },async (each, token) =>
        {
        // foreach (ForumThread each in context.Threads) {
            //// if (each.Posts!= null && each.Posts.Count > 0) return;
            
            Console.WriteLine("Thread {0}, Page {1}", each.Id, 1);
            PostPage posts = await postRepo.GetPostPage(each.ThreadLink, 1);
            each.Posts = new();
            each.Posts.AddRange(posts.ForumPosts);

            if (posts.TotalPages > 2) {
                await Parallel.ForEachAsync(Enumerable.Range(2, posts.TotalPages - 2),
                    new ParallelOptions { MaxDegreeOfParallelism = 5 },
                    async (i, tok) =>
                    {
                        posts = await postRepo.GetPostPage(each.ThreadLink, i);
                        each.Posts.AddRange(posts.ForumPosts);
                        Console.WriteLine("Thread {0}, Page {1}", each.Id, i);
                    }
                );
            }
            
            // for (int i = 2; i < posts.TotalPages; i++) {
            //     posts = await postRepo.GetPostPage(each.ThreadLink, i);
            //     each.Posts.AddRange(posts.ForumPosts);
            //     Console.WriteLine("Thread {0}, Page {1}", each.Id, i);
            // }


        }
        );
            
       await context.SaveChangesAsync();
    }
}