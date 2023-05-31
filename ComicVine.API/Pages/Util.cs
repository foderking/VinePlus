
using Comicvine.Core;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ComicVine.API.Pages;

public record Nav(int CurrentPage, int LastPage, string Path);

public static class Util
{
    public static int LastPage = 16600;

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
                $"{dur.Days} days"
        };
    }

    public static Func<Parsers.Thread, string> GetThreadLink(bool isArchive) {
        return isArchive ? 
            (thread) => $"/archives/thread/{thread.Id}" :
            (thread) => $"/thread?path={thread.Thread.Link}";
    }

    public static string GetThreadRow(int index) {
        return index % 2  == 1? "thread-odd" : "";
    }

    public static Func<string, int, string> NormalThreadNav = (path, page) => $"/thread?path={path}&p={page}";
    public static Func<string, int, string> ArchiveThreadNav = (id, page) => $"/archives/thread/{id}/{page}";
    public static Func<string, int, string> ThreadNav = (path, page) => $"{path}/{page}";

    public static string GetClass(ViewDataDictionary ViewData, string expected) {
        if (expected == (string)(ViewData[SiteKey] ?? "")) {
            return "nav-item nav-highlight";
        }
        else {
            return "nav-item";
        }
    }
    
    public static string TitleKey = "title";
    public static string SiteKey = "site";
    public static string HeaderKey = "headings";
    
    
    public static class Profile
    {
        public static string IsProfile = "key:profile";
        public static string IsImage = "key:image";
        public static string IsPosts = "key:post";
        public static string IsBlog = "key:blog";
        public static string IsThread = "key:thread";

        public static string ProfileKey = "profile:name";
        
        public static string GetLink(ViewDataDictionary ViewData, string path) {
            string user = (string)(ViewData[ProfileKey] ?? "");
            return path + user;
        }
    }
}