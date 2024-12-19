using Comicvine.Core;
using VinePlus.Database;

namespace VinePlus.Web.Pages.Profile;

public class Threads : Navigator<ThreadView>, IForum
{
    private ComicvineContext _context;
    public string UserName = "";

    public Threads(ComicvineContext ctx) {
        _context = ctx;
    }
    
    public void OnGet(string user, int p) {
        UserName = user;
        Entities = Queries.getUsersThreads(_context, UserName, p);
        NavRecord = new(p, 100000, user);
    }

    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/profile/threads/{user}/{page}";
    }

    public Func<ThreadView, string> GetThreadLink() {
        return (thread) => $"/archives/thread/{thread.thread_id}";
    }
}