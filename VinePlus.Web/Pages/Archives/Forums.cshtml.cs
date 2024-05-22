using Comicvine.Core;
using Comicvine.Database;

namespace ComicVine.API.Pages.Archives;

public class Forums : Navigator<Parsers.Thread>, IForum
{
    private ComicvineContext _context;

    public Forums(ComicvineContext context) {
        _context = context;

    }
    
    public void OnGet(int p) {
        Entities = Util.GetArchivedThreads(_context, p);
        NavRecord = new(p, Util.GetThreadsMaxPage(_context), p.ToString());
    }

    public override Func<string, int, string> PageDelegate() {
        return (_, page) => $"/archives/forums/{page}";
    }
    
    public Func<Parsers.Thread, string> GetThreadLink() {
        return (thread) => $"/archives/thread/{thread.Id}?p=1";
    }
}