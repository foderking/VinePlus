using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Archives;

public class Thread : PageModel
{
    private ComicvineContext _context;
    public IEnumerable<Parsers.Post>? Posts;

    public Thread(ComicvineContextFactory factory) {
        _context = factory.CreateDbContext(Array.Empty<string>());
    }
    public void OnGet(int id) {
        Posts = _context.Posts
            .Where(p => p.ThreadId == id)
            .Take(50);
    }
}