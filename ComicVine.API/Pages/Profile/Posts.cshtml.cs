using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Profile;

public class Posts : PageModel
{
    public string User_ = "";
    public IEnumerable<Parsers.Post> Post = Enumerable.Empty<Parsers.Post>();
    private ComicvineContext _context;

    public Posts(ComicvineContext ctx) {
        _context = ctx;
    }
    
    public async Task OnGet(string user) {
        var profile = await Parsers.ProfileParser.ParseDefault($"/profile/{user}");
        User_ = profile.UserName;
        Post = Util.Profile.GetUserPosts(_context, user, 1);
    }
}