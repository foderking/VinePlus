using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComicVine.API.Pages.Archives;

public class Forums : PageModel
{
    private ComicvineContext _context;
    public IEnumerable<Parsers.Thread>? Threads;
    public Nav FNav = new (1, Util.LastPage, "/archives/forums");

    public Forums(ComicvineContext context) {
        _context = context;

    }
    /*
     * - order by id
     * - filter by author
     * - filter by board
     * - filter by type
     */
    public void OnGet(int p) {
        Threads = _context.Threads
            // .OrderByDescending(t =>
            //     t.Posts.Count
            // )
            .OrderBy(t =>
                t.Id
            )
            .Skip(50 * (p - 1))
            .Take(50)
            ;
        
        FNav = FNav with { CurrentPage = p, LastPage = _context.Threads.Count() / 50 };
    }
}