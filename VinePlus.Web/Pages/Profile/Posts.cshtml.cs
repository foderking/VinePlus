using Comicvine.Core;
using VinePlus.Database;

namespace VinePlus.Web.Pages.Profile;

public class Posts(ComicvineContext context) : Pagination<PostSummary>
{
    public string UserName = "";
    
    public void OnGet(string user, int p) {
        UserName = user;
        Entities = Queries.getUserPostSummary(context, user, p);
        NavRecord = new(p, 1000, user);
    }

    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/profile/posts/{user}/{page}";
    }
}