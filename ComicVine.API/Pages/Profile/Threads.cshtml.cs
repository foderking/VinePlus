using Comicvine.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Profile;

public class Threads : PageModel
{
    private Parsers.ISingle<Parsers.Profile> parser;
    public string User_ = "";
    
    public Threads(Parsers.ISingle<Parsers.Profile> p) {
        parser = p;
    }
    public async Task OnGet(string user) {
        var profile = await Parsers.Common.ParseDefault(parser, $"/profile/{user}");
        User_ = profile.UserName;
    }
}