using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Archives;

public class Forums : PageModel
{
    private ComicvineContext _context;
    public IEnumerable<Parsers.Thread>? Threads;
    
    public Forums(ComicvineContextFactory factory) {
        _context = factory.CreateDbContext(Array.Empty<string>());

    }
    public void OnGet(int p) {
        Threads = _context.Threads
            .Skip(50*(p-1))
            .Take(50);
    }
}