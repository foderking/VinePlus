using Comicvine.Core;
using VinePlus.Database;

namespace VinePlus.Web.Pages.Archives;

public class Thread(ComicvineContext context) : Pagination<PostView>
{
    public ThreadHeading thread_heading;

    public void OnGet(int id, int? p) {
        int no_posts = Queries.getPostsMaxPage(context, id);
        thread_heading = Queries.getThreadTitle(context, id);
        Entities = p switch 
        {
            null => Queries.getAllPosts(context, id),
            { } page => Queries.getAllPosts(context, id, page)
        };
        NavRecord = new(p ?? -1, no_posts, id.ToString());
    }

   
    public override Func<string, int, string> PageDelegate() {
        return (id, page) => $"/archives/thread/{id}/?p={page}";
    }
}