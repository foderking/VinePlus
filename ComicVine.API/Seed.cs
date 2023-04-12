using System.Diagnostics;
using System.Net;
using System.Text.Json;
using ComicVine.API.Database;
using ComicVine.API.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using ComicVine.API.Controllers;
using ComicVine.API.Scripts;

namespace ComicVine.API;
using Repository;

public static class Seed
{ 
    static int maxParallel = 10;

    private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
    private static readonly ForumRepository ForumRepo = new();

    private static readonly string ThreadKey = "forum:thread";
    
    //  private static void GetForumsFromJson(string? seedPath, ForumContext context) {
    //     if (File.Exists(seedPath)) {
    //         using FileStream file = File.OpenRead(seedPath);
    //         // Forum? forumThread = JsonSerializer.Deserialize<Forum>(File.ReadAllText(seedPath));
    //         Forum? forumThread = JsonSerializer.Deserialize<Forum>(file);
    //
    //         if (forumThread == null) throw new Exception("Invalid Json file");
    //         // var t = forumThread
    //         //     .threads
    //         //     .GroupBy(each => each.Id, each => each.ThreadTitle,
    //         //         (k, g) => new { i = k, l = g.Count(), z = g.ToArray() })
    //         //     .Where(each => each.l > 1);
    //
    //         context.Threads.AddRange(forumThread.threads);
    //         context.SaveChanges();
    //     }
    //     else throw new Exception("Invalid Seed Path");
    // }
    

     public static async Task CacheForums() {
         Console.WriteLine("[+] Caching Threads"); 
         IDatabase db = redis.GetDatabase();
         
         db.KeyDelete(ThreadKey);

         ForumPage forumPage = await ForumRepo.GetForumPage(1);
         int totalPages = forumPage.TotalPages;
         // int totalPages = 100;
         // string redisHashmap = "forums:hash";

         await Parallel.ForEachAsync(Enumerable.Range(1, totalPages), new ParallelOptions { MaxDegreeOfParallelism = maxParallel }, 
             (page, token) => GetEachForumPage(page, token, async (eachThread, cancToken) =>
                 {
                     string serializedThread = JsonSerializer.Serialize<ForumThread>(eachThread);
                     // return !(await db.SetAddAsync("forums", serializedThread ));
                     return !(await db.SetAddAsync(ThreadKey, serializedThread ));
                     // if (db.HashExists(redisHashmap, eachThread.Id)) {
                     //     
                     // }
                     // else {
                     //     db.HashSetAsync("forums:hash")
                     // }
                 }
             )
         );
     }

     public static async Task GetCachedForums(ForumContext context) { 
         Console.WriteLine("[+] Storing Threads in DB"); 
         IDatabase db = redis.GetDatabase();
         
         IEnumerable<ForumThread> deserializedThreads = db
            .SetScan(ThreadKey)
            .Select(each => JsonSerializer.Deserialize<ForumThread>(each))
            // .Where(each => each != null && context.Threads.Find(each.Id) == null );
            .Where(each => each != null);

        await context.Threads.AddRangeAsync(deserializedThreads);
        // await Parallel.ForEachAsync(db.SetScan("forums"), new ParallelOptions { MaxDegreeOfParallelism = maxParallel },
        //     async (serializedThread, token) =>
        //     {
        //         ForumThread deserializedForum = JsonSerializer.Deserialize<ForumThread>(serializedThread);
        //         if (deserializedForum != null)
        //             await context.Threads.AddAsync(deserializedForum, token);
        //     }
        // );


     }
     
    // public static async Task<IEnumerable<ForumThread>> GetForumsDirect(ForumContext context) {
    //
    //     HashSet<ForumThread> allThreads = new();
    //     
    //
    //     ForumPage forumPage = await ForumRepo.GetForumPage(1);
    //     int totalPages = forumPage.TotalPages;
    //
    //     allThreads.UnionWith(forumPage.ForumThreads);
    //     // await context.Threads.AddRangeAsync(forumPage.ForumThreads);
    //     
    //     await Parallel.ForEachAsync(Enumerable.Range(2, totalPages), new ParallelOptions { MaxDegreeOfParallelism = maxParallel },
    //         async (page, token) =>
    //         {
    //             forumPage = await ForumRepo.GetForumPage(page);
    //             Console.WriteLine("Parsed Page {0}", forumPage.PageNo);
    //             if (forumPage.ForumThreads.Length != 0)
    //                 allThreads.UnionWith(forumPage.ForumThreads);
    //                 // foreach (ForumThread thread in forumPage.ForumThreads) {
    //                 //     if (await context.Threads.FindAsync(new object?[] { thread.Id }, cancellationToken: token) == null) {
    //                 //         await context.Threads.AddAsync(thread, token);
    //                 //     }
    //                 //     else {
    //                 //         Console.WriteLine("Found duplicate: Id {0}", thread.Id);
    //                 //     }
    //                 // }
    //         }
    //     );
    //     return allThreads;
    // }

    public static async ValueTask GetEachForumPage(int pageNo, CancellationToken token, Func<ForumThread, CancellationToken, Task<bool>> UpdateRedisFunc) {
        try {

            ForumPage forumPage = await ForumRepo.GetForumPage(pageNo);
            Console.WriteLine("Parsed Page {0}", forumPage.PageNo);

            foreach (ForumThread eachThread in forumPage.ForumThreads) {
                bool dupl = await UpdateRedisFunc(eachThread, token);
                Console.WriteLine("Thread {0}{1}", eachThread.Id, dupl ? ": Duplicate" : "");
            }
        }
        catch (HttpRequestException e) {
            if (e.StatusCode == HttpStatusCode.TooManyRequests) {
                Console.WriteLine("[+] Throttling page {0}", pageNo);
                maxParallel = Math.Max(5, maxParallel / 2);
            }
            else {
                Console.WriteLine("Error, Restarting page {0}; Message: {1}", pageNo, e.Message);
            }

            await GetEachForumPage(pageNo, token, UpdateRedisFunc);
        }
    }
    
    public static void InitializeForums(IServiceProvider serviceProvider, string? seedPath = null) {
        using ForumContext context = new ForumContext(
            serviceProvider.GetRequiredService<DbContextOptions<ForumContext>>()
        );

        CacheForums().Wait();
        GetCachedForums(context).Wait();
        // if (context.Threads.Any()) return;
        //
        // // GetForumsFromJson(seedPath, context);
        // var threads = GetForumsDirect(context).Result;
        // context.Threads.AddRange(threads);
        context.SaveChanges();
    }



    public static async Task InitializePosts(IServiceProvider serviceProvider) {
        await using ForumContext context = new ForumContext(
            serviceProvider.GetRequiredService<DbContextOptions<ForumContext>>()
        );
        
        PostRepository postRepo = new ();
        if (!context.Threads.Any()) return;

        // var thread = context.Threads.First();
        // PostPage? posts = await postRepo.GetPostPage(thread.ThreadLink, 1);
        // if (posts == null) return;
        // var allPosts = posts.ForumPosts.Select(each =>
        // {
        //     each.Thread = thread;
        //     each.ThreadId = thread.Id;
        //     return each;
        // });
        //
        // // thread.Posts = new();
        // context.Posts.AddRange(allPosts);
        // foreach (ForumPost postsForumPost in posts.ForumPosts) {
        //     Console.WriteLine(postsForumPost.PostNo);
        // }
        try {
        
            await Parallel.ForEachAsync(context.Threads, new ParallelOptions { MaxDegreeOfParallelism = 10 },
                async (forumThread, token) =>
                {
                    // foreach (ForumThread each in context.Threads) {
                    //// if (each.Posts!= null && each.Posts.Count > 0) return;
        
                    Console.WriteLine("Thread {0}, Page {1}", forumThread.Id, 1);
                    PostPage posts = await postRepo.GetPostPage(forumThread.ThreadLink, 1);
                    // if (posts == null) return;
                    
                     var allPosts = posts.ForumPosts.Select(each =>
                    {
                        each.Thread = forumThread;
                        each.ThreadId = forumThread.Id;
                        return each;
                    });       
                    await context.Posts.AddRangeAsync(allPosts, token);
                     
                    //
                    // if (posts.TotalPages > 1) {
                    //     // await Parallel.ForEachAsync(Enumerable.Range(2, posts.TotalPages - 2),
                    //     //     new ParallelOptions { MaxDegreeOfParallelism = 50 },
                    //     //     async (i, tok) =>
                    //         for (int i = 2; i < posts.TotalPages; i++) 
                    //         {
                    //             posts = await postRepo.GetPostPage(each.ThreadLink, i);
                    //             if (posts == null) return;
                    //             
                    //             each.Posts.AddRange(posts.ForumPosts);
                    //             Console.WriteLine("Thread {0}, Page {1}", each.Id, i);
                    //         }
                    //     // );
                    // }
        
                    //     posts = await postRepo.GetPostPage(each.ThreadLink, i);
                    //     each.Posts.AddRange(posts.ForumPosts);
                    //     Console.WriteLine("Thread {0}, Page {1}", each.Id, i);
                    // }
        
        
                }
            );
        
        }
        catch (Exception e) {
            Console.WriteLine("Got an erorr {0}, writing to db", e.Message);
            
        }

        await context.SaveChangesAsync();
    }
}