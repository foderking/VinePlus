using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Profile;

public class Posts : Navigator<Parsers.Post>
{
    public string UserName = "";
    private ComicvineContext _context;

    public Posts(ComicvineContext ctx) {
        _context = ctx;
    }
    
    public void OnGet(string user, int p) {
        UserName = user;
        Entities = Util.Profile.GetUserPosts(_context, user, p);
        NavRecord = new(p, 1000, user);
    }

    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/profile/posts/{user}/{page}";
    }
}