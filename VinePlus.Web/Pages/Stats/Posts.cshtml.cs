using Comicvine.Core;
using VinePlus.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinePlus.Web.Pages.Stats;

public class Posts(ComicvineContext context): Pagination<ThreadsSummary>
{
    public void OnGet(string user, int p=1) {
        Entities = Queries.getThreadsPosted(context, user, p);
        NavRecord = new(p, 1000, user);
    }
    
    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/stats/posts/{user}?p={page}";
    }
}