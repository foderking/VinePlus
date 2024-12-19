using Comicvine.Core;
using VinePlus.Database;

namespace VinePlus.Web.Pages.Archives;

public class Thread : Pagination<PostView>
{
    private ComicvineContext _context;
    public Parsers.Thread? CurrentThread;

    public Thread(ComicvineContext context) {
        _context = context;
    }
    
    public void OnGet(int id, int? p) {
        CurrentThread = _context.Threads.Find(id);
        Entities = p switch 
        {
            null => Queries.getAllPosts(_context, id),
            { } page => Queries.getAllPosts(_context, id, page)
        };
        NavRecord = new(p ?? -1, CurrentThread?.LastPostPage ?? 1, id.ToString());
    }

   
    public override Func<string, int, string> PageDelegate() {
        return (id, page) => $"/archives/thread/{id}/?p={page}";
    }
}