
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace ComicVine.API.Pages;

public record Nav(int CurrentPage, int LastPage, string DelegateParam);

public abstract class Navigator<T>: PageModel
{
    public IEnumerable<T> Entities = Enumerable.Empty<T>();
    public Nav NavRecord = new(0, 0, "");
    public abstract Func<string, int, string> PageDelegate();
}

public interface IForum
{
    public Func<Parsers.Thread, string> GetThreadLink();
}

public static class ProfileHighlight
{
    public const string Main = "profile";
    public const string Images = "images";
    public const string Blogs = "blogs";
    public const string Threads = "threads";
    public const string Posts = "posts";
}

public static class Keys
{
    public const string Highlight = "highlight";
    public const string Headings  = "headings";
    public const string Title  = "title";
    public const string ProfileName  = "profile-name";
    public const string ProfileHasBlog  = "profile-blog";
    public const string ProfileHasImage  = "profile-image";
}

public static class MainHighlight
{
    public const string Vine = "vine";
    public const string Archives = "archives";
    public const string Search = "search";
    public const string Stats = "stats";
}

public enum SortForumBy
{
    DateCreated, // sorts by id
    NoViews,
    NoPosts
}

public class ImgData
{
    [JsonPropertyName("dateCreated")]
    public string DateCreated {get; set; } = "";
    [JsonPropertyName("gallery")]
    public string Gallery { get; set; } = "";
    [JsonPropertyName("original")]
    public string Original { get; set; } = "";
}

public static class Util
{
    public static int ThreadPerPage = 50;
    public static int PostsPerPage = 50;
    
    public static string GetDuration(DateTime current, DateTime date) {
        TimeSpan dur = current - date;
        return dur.Days switch
        {
            0 when dur.Hours == 0 && dur.Minutes == 0 && dur.Seconds == 1=> 
                $"{dur.Seconds} second",
            0 when dur.Hours == 0 && dur.Minutes == 0 => 
                $"{dur.Seconds} seconds",
            0 when dur.Hours == 0 && dur.Minutes == 1 => 
                $"{dur.Minutes} minute",
            0 when dur.Hours == 0 => 
                $"{dur.Minutes} minutes",
            0 when dur.Hours == 1 => 
                $"{dur.Hours} hour",
            0 => 
                $"{dur.Hours} hours",
            1 => 
                $"{dur.Days} day",
            _ => 
                date.ToLongDateString()
        };
    }

    public static string GetThreadRow(int index) {
        return index % 2  == 1? "thread-odd" : "";
    }
     
    public static IEnumerable<Parsers.Thread> getUsersThreads(ComicvineContext context, string user, int page=1) {
        return context
            .Threads
            .Where(thread => thread.Creator.Text == user)
            .OrderByDescending(thread => thread.TotalPosts)
            .ThenBy(thread => thread.TotalView )
            .Skip(ThreadPerPage * (page-1))
            .Take(ThreadPerPage);
    }

    public static IEnumerable<Parsers.Post> GetUserPosts(ComicvineContext context, string user, int page=1) {
        return context
            .Posts
            .Where(posts => posts.Creator.Text == user)
            // .OrderByDescending(each => each.Id)
            .Skip(PostsPerPage * (page-1))
            .Take(PostsPerPage)
            ;
    }

    public record PostWithThread(Parsers.Post P, Parsers.Thread T);
    
    public static IEnumerable<PostWithThread> getUserPostsWithThread(ComicvineContext context, string user, int page=1) {
        return context
            .Posts
            .Where(posts => posts.Creator.Text == user)
            .OrderByDescending(each => each.Created)
            .Join(
                context.Threads,
                post => post.ThreadId,
                thread => thread.Id,
                (post, thread) => new PostWithThread(post, thread)
            )
            .Skip(PostsPerPage * (page-1))
            .Take(PostsPerPage);
    }

    public record ThreadsPosted(int thread_id, string thread_text, int no_posts);

    public static IEnumerable<ThreadsPosted> getThreadsPosted(ComicvineContext context, string user, int page = 1) {
        return context
            .Posts
            .Where(posts => posts.Creator.Text == user)
            .Join(
                context.Threads,
                post => post.ThreadId,
                thread => thread.Id,
                (post, thread) => new { Thread = thread, Post = post }
            )
            .GroupBy(x => new { x.Thread.Id, x.Thread.Thread.Text })
            .Select(x => new ThreadsPosted(x.Key.Id, x.Key.Text, x.Count()));
        //.OrderByDescending(x => x.no_posts)
        //.Skip(PostsPerPage * (page-1))
        //.Take(PostsPerPage);
    }

    public static IEnumerable<Parsers.Thread> getArchivedThreads(ComicvineContext context, int page, SortForumBy sort_by = SortForumBy.DateCreated) {
        return sort_by switch
        {
            SortForumBy.DateCreated => 
                context
                .Threads
                .OrderByDescending(thread => thread.Id)
                .Skip(ThreadPerPage * (page - 1))
                .Take(ThreadPerPage),
            SortForumBy.NoViews => 
                context
                .Threads
                .OrderByDescending(thread => thread.TotalView)
                .Skip(ThreadPerPage * (page - 1))
                .Take(ThreadPerPage),
            SortForumBy.NoPosts => 
                context
                .Threads
                .OrderByDescending(thread => thread.TotalPosts)
                .Skip(ThreadPerPage * (page - 1))
                .Take(ThreadPerPage),
            _ => throw new ArgumentOutOfRangeException(nameof(sort_by), sort_by, null)
        };
    }

    public static int GetThreadsMaxPage(ComicvineContext context) {
        return context.Threads.Count() / ThreadPerPage + 1;
    }

    public static string GetHighlightClass(ViewDataDictionary viewData, string expected) {
        if (expected == (string)(viewData[Keys.Highlight] ?? "")) {
            return "nav-item nav-highlight";
        }
        return "nav-item";
    }  
    
    
    public static class Profile
    {
        public static string GetLink(ViewDataDictionary viewData, string path) {
            string user = (string)(viewData[Keys.ProfileName] ?? "");
            return path + user;
        }

    }


    private class ImgResponse
    {
        [JsonPropertyName("images")] 
        public IEnumerable<ImgData> Images { get; set; } = Enumerable.Empty<ImgData>();
    }

    public static class Image
    {
        public static int BatchSize = 36;
        
        public static async Task<IEnumerable<ImgData>> GetImages(Parsers.Image data, int offset) {

            using HttpClient client = new();
            string response = await client.GetStringAsync(
                $"https://www.comicvine.gamespot.com/js/image-data.json?images={data.GalleryId}&object={data.ObjectId}&start={offset * BatchSize}&count={BatchSize}"
            );
            ImgResponse? res = JsonSerializer.Deserialize<ImgResponse>(response);
            return res!.Images;
        }
    }

    public static string GetPostClass(Parsers.Post post) {
        if (post.IsDeleted) {
            return "post-item post-deleted";
        }
        else {
            return "post-item";
        }
    }

    public static class Search
    {
        public static IEnumerable<Parsers.Thread> searchThreads(ComicvineContext context, string query) {
            return context
                .Threads
                .Where(thread => 
                    EF.Functions
                        .ToTsVector(thread.Thread.Text)
                        .Matches(EF.Functions.WebSearchToTsQuery(query))
                );
        }
        public static IEnumerable<Parsers.Thread> searchThreadsFromUser(ComicvineContext context, string query, string creator) {
            return context
                .Threads
                .Where(thread => 
                    EF.Functions
                        .ToTsVector(thread.Thread.Text)
                        .Matches(EF.Functions.WebSearchToTsQuery(query))
                    && thread.Creator.Link.EndsWith(creator + "/")
                );
        }
    }
}