using Comicvine.Core;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages;

public class Forums : PageModel
{
    public IEnumerable<Parsers.Thread> Threads = Enumerable.Empty<Parsers.Thread>();
    public Nav FNav = new (1, Util.LastPage, "/forums");
    public Func<string, int, string> Navigation = (path, page) => $"{path}/{page}";

    public async Task OnGet(int p) {
        HtmlNode rootNode = await Net.getNodeFromPage("/forums", p);
        Threads = Parsers.ThreadParser.ParseSingle(rootNode);
        int last = Parsers.Common.parsePageEnd(rootNode);
        FNav = FNav with { CurrentPage = p, LastPage = last};
    }
}