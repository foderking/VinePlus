using Comicvine.Core;
using Comicvine.Database;

namespace ComicVine.API.Pages.Archives;

public class Thread : Navigator<Parsers.Post>
{
    private ComicvineContext _context;
    public Parsers.Thread? Thread_;

    public Thread(ComicvineContext context) {
        _context = context;
    }
    public void OnGet(int id, int p) {
        Thread_ = _context.Threads.Find(id);
        Entities = 
            _context.Posts
            .Where(_p => _p.ThreadId == id)
            .OrderBy(_p => _p.PostNo)
            .Skip((p-1)*50)
            .Take(50);
        NavRecord = new(p, Thread_?.LastPostPage ?? 1, id.ToString());
    }

    public override Func<string, int, string> PageDelegate() {
        return (id, page) => $"/archives/thread/{id}/{page}";
    }
}