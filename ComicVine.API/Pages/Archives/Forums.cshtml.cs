using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComicVine.API.Pages.Archives;

public class Forums : PageModel
{
    private ComicvineContext _context;
    public IEnumerable<Parsers.Thread>? Threads;
    public string GetLock(bool isLocked) {
        return
            isLocked
            ? "<img src='https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-lock-16x16.png'>"
            : "";
    }
    
    public string GetPin(bool isPinned) {
        return
            isPinned
            ? "<img src='https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-pin-16x16.png'>"
            : "";
    }
    public Forums(ComicvineContextFactory factory) {
        _context = factory.CreateDbContext(Array.Empty<string>());

    }
    public void OnGet(int p) {
        Threads = _context.Threads
            .OrderByDescending(t =>
                t.Posts.Count
                // t.Posts.OrderByDescending(pp => pp.Created).First().Created 
            )
            .Skip(50 * (p - 1))
            .Take(50)
            .Include(t => t.Posts)
            ;
    }
}