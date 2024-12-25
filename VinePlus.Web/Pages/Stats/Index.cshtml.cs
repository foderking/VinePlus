using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VinePlus.Database;

namespace VinePlus.Web.Pages.Stats;

[IgnoreAntiforgeryToken(Order = 1001)] // disables anti-forgery token requirement   https://www.learnrazorpages.com/security/request-verification
public class Index(ComicvineContext context) : PageModel
{
    public IEnumerable<ThreadView> top_threads;
    public IEnumerable<BoardView> board_summary;
    public int total_threads;
    public int total_posts;
     
    public void OnGet() {
        top_threads = Queries.getArchivedThreads(context, 1, SortForumBy.NoPosts).Take(10);
        total_posts = Queries.getTotalPosts(context);
        total_threads = Queries.getTotalThreads(context);
        board_summary = Queries.getBoardsViews(context);
    }

    public IActionResult OnPost(string searchQuery) {
        return Redirect($"/stats/user?username={searchQuery.Trim()}");
    }
}