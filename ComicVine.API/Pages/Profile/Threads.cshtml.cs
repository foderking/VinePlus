using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Profile;

public class Threads : Navigator<Parsers.Thread>, IForum
{
    public string UserName = "";
    // public IEnumerable<Parsers.Thread> Thread = Enumerable.Empty<Parsers.Thread>();
    private ComicvineContext _context;
    // public Nav FNav = new (1, 1, "/forums");

    public Threads(ComicvineContext ctx) {
        _context = ctx;
    }
    
    public void OnGet(string user, int p) {
        UserName = user;
        Entities = Util.Profile.GetUsersThreads(_context, UserName, p);
        NavRecord = new(p, 100000, user);
    }

    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/profile/threads/{user}/{page}";
    }

    public Func<Parsers.Thread, string> GetThreadLink() {
        return (thread) => $"/archives/thread/{thread.Id}";
    }
}