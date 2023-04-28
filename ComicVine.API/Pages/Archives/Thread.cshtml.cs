using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Archives;

public class Thread : PageModel
{
    private ComicvineContext _context;
    public IEnumerable<Parsers.Post>? Posts;
    public Parsers.Thread? Thread_;

    public Thread(ComicvineContextFactory factory) {
        _context = factory.CreateDbContext(Array.Empty<string>());
    }
    public void OnGet(int id, int p) {
        Thread_ = _context.Threads.Find(id);
        Posts = _context.Posts
            .OrderBy(p => p.PostNo)
            .Where(p => p.ThreadId == id)
            .Skip((p-1)*50)
            .Take(50);
    }
}