using Microsoft.AspNetCore.Mvc.RazorPages;
using VinePlus.Database;

namespace VinePlus.Web.Pages.Stats;

public class Index(ComicvineContext context) : PageModel
{
    public IEnumerable<ThreadView> top_threads;
    public IEnumerable<ProfilePostView> top_users;
    public void OnGet() {
        top_threads = Queries.getArchivedThreads(context, 1, SortForumBy.NoPosts).Take(10);
        top_users = Queries.getPostViews(context);
    }
}