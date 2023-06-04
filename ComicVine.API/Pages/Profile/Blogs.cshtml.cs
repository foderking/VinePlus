using Comicvine.Core;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Profile;

public class Blogs : Navigator<Parsers.Blog>
{
    public string UserProfile = "";

    public async Task OnGet(string user, int p) {
        var profile = await Parsers.ProfileParser.ParseDefault($"/profile/{user}");
        HtmlNode node = await Net.getNodeFromPage($"/profile/{user}/blog", p);
        int last = Parsers.BlogParser.ParseEnd(node);
        Entities = Parsers.BlogParser.ParseSingle(node);
        UserProfile = profile.UserName;
        NavRecord = new(p, last, profile.UserName);
    }

    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/profile/blogs/{user}/{page}";
    }
}