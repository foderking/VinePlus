using Comicvine.Core;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages;

public class Thread : PageModel
{
    public IEnumerable<Parsers.Post>? Posts;
    public string ThreadTitle = "";
    public Nav FNav = new (1, 1, "/forums");

    public async Task OnGet(string path, int p=1) {;
        HtmlNode node = await Net.getNodeFromPage(path, p);
        Posts = Parsers.PostParser.ParseSingle(node);//await Parsers.Common.ParseSingle(_parser, p, path);
        ThreadTitle = Parsers.Common.getThreadTitle(node);
        int last = Parsers.PostParser.ParseEnd(node);
        FNav = new Nav(Path: path, CurrentPage: p, LastPage: last);
    }
}