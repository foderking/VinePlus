
using Comicvine.Core;

namespace ComicVine.API.Pages;

public record Nav(int CurrentPage, int LastPage, string Path);

public static class Util
{
    public static int LastPage = 16600;


    public static string GetThreadLink(bool isArchive, Parsers.Thread thread) {
        return isArchive ? 
            $"/archives/thread/{thread.Id}" : 
            $"/thread?path={thread.Thread.Link}";
    }

    public static string GetThreadRow(int index) {
        return index % 2  == 1? "thread-odd" : "";
    }
}