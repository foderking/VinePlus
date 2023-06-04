
using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ComicVine.API.Pages;

public record Nav(int CurrentPage, int LastPage, string DelegatParam);

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
}

public static class MainHighlight
{
    public const string Vine = "vine";
    public const string Archives = "archives";
    public const string Search = "search";
    public const string Stats = "stats";
}

public static class Util
{
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
   
    public static class Profile
    {
        public static string GetLink(ViewDataDictionary ViewData, string path) {
            string user = (string)(ViewData[Keys.ProfileName] ?? "");
            return path + user;
        }

    }
    
    public static IEnumerable<Parsers.Thread> GetUsersThreads(ComicvineContext context, string user, int page=1) {
        return context
            .Threads
            .Where(thread => thread.Creator.Text == user)
            .OrderByDescending(each => each.TotalPosts)
            .Skip((page-1)*50)
            .Take(50)
            ;
    }

    public static IEnumerable<Parsers.Post> GetUserPosts(ComicvineContext context, string user, int page=1) {
        return context
            .Posts
            .Where(posts => posts.Creator.Text == user)
            // .OrderByDescending(each => each.Id)
            .Skip((page-1)*50)
            .Take(50)
            ;
    }
    
    public static string GetHighlightClass(ViewDataDictionary ViewData, string expected) {
        if (expected == (string)(ViewData[Keys.Highlight] ?? "")) {
            return "nav-item nav-highlight";
        }
        return "nav-item";
    }
}