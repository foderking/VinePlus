using Comicvine.Core;
using Comicvine.Database;

namespace VinePlus.Web.Pages.Profile;

public class Threads : Navigator<Parsers.Thread>, IForum
{
    private ComicvineContext _context;
    public string UserName = "";

    public Threads(ComicvineContext ctx) {
        _context = ctx;
    }
    
    public void OnGet(string user, int p) {
        UserName = user;
        Entities = Util.getUsersThreads(_context, UserName, p);
        NavRecord = new(p, 100000, user);
    }

    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/profile/threads/{user}/{page}";
    }

    public Func<Parsers.Thread, string> GetThreadLink() {
        return (thread) => $"/archives/thread/{thread.Id}";
    }
}