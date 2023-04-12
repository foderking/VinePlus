using System.Text.Json;
using ComicVine.API.Models;
using ComicVine.API.Repository.Parsers;
using HtmlAgilityPack;


namespace ComicVine.API.Scripts;
using API.Repository;

class Forum
{
    public List<ForumThread> threads { get; set; } = new();
}

public static class ScrapeThreads
{
    public static async Task<ForumThread[]> GetThreads(int pageNo) {
        await using Stream stream = await Repository.Repository.GetStream($"/forums", new ( new []
        {
            new KeyValuePair<string, string>("page", pageNo.ToString())
        }) );
        HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
        HtmlNode wrapperNode = MainParser.GetWrapperNode(rootNode);
        return ForumParser.ParseThreads(wrapperNode)!;
    }

    public static async Task Scrape() {
        await using FileStream stream = File.Create("thread.json");
        Forum forum = new();
        forum.threads = new List<ForumThread>();
        // forum.threads = (await GetThreads(1)).ToList();
        // for (int i = 1; i < 100; i++) {
        //     forum.threads.AddRange(await GetThreads(i+1));
        //     Console.WriteLine("Thread {0} read", i);
        // }

        // Task.Factory.StartNew(
        //     () =>
        await Parallel.ForEachAsync(Enumerable.Range(1, 100), new ParallelOptions { MaxDegreeOfParallelism = 20 },

            async (i, a) =>
            {
                forum.threads.AddRange(await GetThreads(i));
                Console.WriteLine("Thread {0} read", i);

            });
        // );
        await JsonSerializer.SerializeAsync(stream, forum);
    }
}