using Comicvine.Core;
using VinePlus.Database;

namespace VinePlus.Web.Pages.Archives;

public class Forums : Pagination<ThreadView>, IForum
{
    private ComicvineContext _context;
    private string sort_param = "";

    public Forums(ComicvineContext context) {
        _context = context;
    }
    
    public void OnGet(int p, string? sort_by) {
        sort_param = sort_by!=null ? $"?sort_by={sort_by}" : ""; 
        SortForumBy sort = sort_by switch
        {
            "views" => SortForumBy.NoViews,
            "posts" => SortForumBy.NoPosts,
            _ => SortForumBy.DateCreated
        };
        Entities = Queries.getArchivedThreads(_context, p, sort);
        NavRecord = new(p, Queries.getThreadsMaxPage(_context), p.ToString());
    }

    public override Func<string, int, string> PageDelegate() {
        return (_, page) => $"/archives/forums/{page}/{sort_param}";
    }
    
    public Func<ThreadView, string> GetThreadLink() {
        return (thread) => $"/archives/thread/{thread.thread_id}?p=1";
    }
}