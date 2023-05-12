
using Comicvine.Core;

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

    public static string GetThreadLink(bool isArchive, Parsers.Thread thread) {
        return isArchive ? 
            $"/archives/thread/{thread.Id}" : 
            $"/thread?path={thread.Thread.Link}";
    }

    public static string GetThreadRow(int index) {
        return index % 2  == 1? "thread-odd" : "";
    }
}