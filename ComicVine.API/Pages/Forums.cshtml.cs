using Comicvine.Core;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages;

public class Forums : PageModel
{
    private Parsers.IMultiple<Parsers.Thread> _parser;
    public IEnumerable<Parsers.Thread>? Threads;
    public Nav FNav = new (1, Util.LastPage, "/forums");
    public Func<string, int, string> Navigation = (path, page) => $"{path}/{page}";

    public Forums(Parsers.IMultiple<Parsers.Thread> p) {
        _parser = p;
    }

    public async Task OnGet(int p) {
        HtmlNode rootNode = await Net.getNodeFromPage("/forums", p);
        Threads = _parser.ParseSingle(rootNode);
        int last = Parsers.Common.parsePageEnd(rootNode);
        FNav = FNav with { CurrentPage = p, LastPage = last};
    }
}