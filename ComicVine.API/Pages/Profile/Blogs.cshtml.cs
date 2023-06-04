using Comicvine.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Profile;

public class Blogs : PageModel
{
    public string User_ = "";
    public IEnumerable<Parsers.Blog> Blog = Enumerable.Empty<Parsers.Blog>();

    public async Task OnGet(string user) {
        var profile = await Parsers.ProfileParser.ParseDefault($"/profile/{user}");
        Blog = await Parsers.BlogParser.ParsePage(1, $"/profile/{user}/blog");
        User_ = profile.UserName;
    }
}