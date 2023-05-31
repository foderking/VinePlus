using Comicvine.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Profile;

public class Images : PageModel
{
    public string User_ = "";
    
    public async Task OnGet(string user) {
        var profile = await Parsers.ProfileParser.ParseDefault($"/profile/{user}");
        User_ = profile.UserName;
    }
}