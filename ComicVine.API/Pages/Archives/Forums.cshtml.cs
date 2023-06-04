using Comicvine.Core;
using Comicvine.Database;

namespace ComicVine.API.Pages.Archives;

public class Forums : Navigator<Parsers.Thread>, IForum
{
    private ComicvineContext _context;

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
        Entities = 
            _context.Threads
            .OrderBy(t => t.Id)
            .Skip(50 * (p - 1))
            .Take(50)
            ;
        
        NavRecord = new(p, _context.Threads.Count() / 50, p.ToString());
    }

    public override Func<string, int, string> PageDelegate() {
        return (_, page) => $"/archives/forums/{page}";
    }
    
    public Func<Parsers.Thread, string> GetThreadLink() {
        return (thread) => $"/archives/thread/{thread.Id}";
    }
}