using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Search;

public class Results : PageModel, IForum
{
    public IEnumerable<Parsers.Thread> ThreadResult = Enumerable.Empty<Parsers.Thread>();
    private ComicvineContext _context;

    public Results(ComicvineContext ctx) {
        _context = ctx;
    }
    public void OnGet(bool searchPost, string query, string? creator) {
        ThreadResult = creator switch
        {
            null => Util.Search.SearchThreads(_context, query),
            _ => Util.Search.SearchThreadsFromUser(_context, query, creator!)
        };

    }

    public Func<Parsers.Thread, string> GetThreadLink() {
        return (thread) => $"/archives/thread/{thread.Id}";
    }
}