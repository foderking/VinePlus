using Comicvine.Core;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinePlus.Web.Pages.Profile;

public class Blogs : Pagination<Parsers.Blog>
{
    public string UserProfile = "";

    public async Task OnGet(string user, int p) {
        try
        {
            var profile = await Parsers.ProfileParser.ParseDefault($"/profile/{user}");
            HtmlNode node = await Net.getNodeFromPage($"/profile/{user}/blog", p);
            int last = Parsers.BlogParser.ParseEnd(node);
            Entities = Parsers.BlogParser.ParseSingle(node);
            UserProfile = profile.UserName;
            NavRecord = new(p, last, profile.UserName);
        }
        catch
        {
            NavRecord = new(1, 1, user);
            if (user.StartsWith("deactivated")) {
                /* needed for navigation to work for deactivated accounts */
                UserProfile = user;
            }
        }
    }

    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/profile/blogs/{user}/{page}";
    }
}