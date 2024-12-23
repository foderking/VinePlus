using Comicvine.Core;
using VinePlus.Database;

namespace VinePlus.Web.Pages.Profile;

public class Threads(ComicvineContext context) : Pagination<ThreadView>, IForum
{
    public string UserName = "";

    public void OnGet(string user, int p) {
        UserName = user;
        Entities = Queries.getUsersThreads(context, UserName, p);
        NavRecord = new(p, 100000, user);
    }

    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/profile/threads/{user}/{page}";
    }

    public Func<ThreadView, string> GetThreadLink() {
        return (thread) => $"/archives/thread/{thread.thread_id}?p=1";
    }
}