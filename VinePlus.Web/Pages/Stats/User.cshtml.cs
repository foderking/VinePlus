using Microsoft.AspNetCore.Mvc.RazorPages;
using VinePlus.Database;

namespace VinePlus.Web.Pages.Stats;

public class User(ComicvineContext context) : PageModel
{
    public int post_count;
    public int thread_count;
    public string user;
    public void OnGet(string username) {
        user = username.Trim();
        thread_count = Queries.getUserThreadCount(context, user);
        post_count = Queries.getUserPostCount(context, user);
    }
}