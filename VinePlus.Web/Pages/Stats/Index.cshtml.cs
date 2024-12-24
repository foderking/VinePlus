using Microsoft.AspNetCore.Mvc.RazorPages;
using VinePlus.Database;

namespace VinePlus.Web.Pages.Stats;

public class Index(ComicvineContext context) : PageModel
{
    public IEnumerable<ThreadView> top10_threads;
    public void OnGet() {
        top10_threads = Queries.getArchivedThreads(context, 1, SortForumBy.NoPosts).Take(10);
    }
}