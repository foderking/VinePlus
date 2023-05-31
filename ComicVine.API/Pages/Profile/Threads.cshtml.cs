using Comicvine.Core;
using Comicvine.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Profile;

public class Threads : PageModel
{
    public string User_ = "";
    public IEnumerable<Parsers.Thread> Thread = Enumerable.Empty<Parsers.Thread>();
    private ComicvineContext _context;

    public Threads(ComicvineContext ctx) {
        _context = ctx;
    }
    public async Task OnGet(string user) {
        var profile = await Parsers.ProfileParser.ParseDefault($"/profile/{user}");
        User_ = profile.UserName;
        Thread = Util.Profile.GetUsersThreads(_context, profile.UserName, 1);
    }
}