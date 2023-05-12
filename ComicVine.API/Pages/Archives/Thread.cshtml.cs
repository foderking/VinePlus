using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Archives;

public class Thread : PageModel
{
    private ComicvineContext _context;
    public IEnumerable<Parsers.Post>? Posts;
    public Parsers.Thread? Thread_;
    public Nav FNav = new (1, 1, "/forums");

    public Thread(ComicvineContextFactory factory) {
        _context = factory.CreateDbContext(Array.Empty<string>());
    }
    public void OnGet(int id, int p) {
        Thread_ = _context.Threads.Find(id);
        Posts = _context.Posts
            .Where(p => p.ThreadId == id)
            .OrderBy(p => p.PostNo)
            .Skip((p-1)*50)
            .Take(50);
        FNav = new(p, Thread_?.LastPostPage ?? 1, id.ToString());
    }
}