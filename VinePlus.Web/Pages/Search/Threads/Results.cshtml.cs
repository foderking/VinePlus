using Comicvine.Core;
using VinePlus.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinePlus.Web.Pages.Search;

public class Results(ComicvineContext context) : Pagination<ThreadView>, IForum
{
    public void OnGet(bool searchPost, string query, string? creator, int p) {
        string s_query = $"searchPost={searchPost}&query={query}" + (creator==null ? "" : "&creator=takenstew22");
        NavRecord = new(p, int.MaxValue, s_query);
        Entities = creator switch
        {
            null => Queries.searchThreads(context, query, p),
            _ => Queries.searchThreadsFromUser(context, query, creator, p)
        };
    }

    public Func<ThreadView, string> GetThreadLink() {
        return (thread) => $"/archives/thread/{thread.thread_id}";
    }
    public override Func<string, int, string> PageDelegate() {
        return (s_query, page) => $"/search/threads/results?{s_query}&p={page}";
    }
}