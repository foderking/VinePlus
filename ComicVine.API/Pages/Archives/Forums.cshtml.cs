using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComicVine.API.Pages.Archives;

public class Forums : PageModel
{
    private ComicvineContext _context;
    public IEnumerable<Parsers.Thread>? Threads;
    public int Page=1;
    public int LastPage=Util.LastPage;

    public Forums(ComicvineContextFactory factory) {
        _context = factory.CreateDbContext(Array.Empty<string>());

    }
    public void OnGet(int p) {
        Page = p;
        Threads = _context.Threads
            .OrderByDescending(t =>
                t.Posts.Count
                // t.Posts.OrderByDescending(pp => pp.Created).First().Created 
            )
            .Skip(50 * (p - 1))
            .Take(50)
            .Include(t => t.Posts)
            ;
        LastPage = _context.Posts.Count() / 50;
    }
}