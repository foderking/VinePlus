using Comicvine.Core;
using VinePlus.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinePlus.Web.Pages.Stats;

public class VisitedThread : PageModel
{
    private ComicvineContext _context;
    public IEnumerable<Parsers.Thread> PostedThreads = Enumerable.Empty<Parsers.Thread>();

    public VisitedThread(ComicvineContext ctx) {
        _context = ctx;
    }
    
    public void OnGet(string user) {
        PostedThreads = GetThreadUserHasPosted(user);
    }

    public IEnumerable<Parsers.Thread> GetThreadUserHasPosted(string user) {
        return _context
            .Threads
            .Where(each => 
                each.Posts.Any(
                    post => post.Creator.Link == $"/profile/{user}/"
                )
            );
    }
}