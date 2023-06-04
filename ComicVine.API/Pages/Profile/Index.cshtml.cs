using Comicvine.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Profile;

public class Index : PageModel
{
    public string UserName = "";
    
    public async Task OnGet(string user) {
        var profile = await Parsers.ProfileParser.ParseDefault($"/profile/{user}");
        UserName = profile.UserName;
    }
}