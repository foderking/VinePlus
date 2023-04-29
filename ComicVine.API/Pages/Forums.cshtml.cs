using Comicvine.Core;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages;

public class Forums : PageModel
{
    private Parsers.IMultiple<Parsers.Thread> _parser;
    public IEnumerable<Parsers.Thread>? Threads;
    public int Page=1;
    public int LastPage=Util.LastPage;

    public Forums(Parsers.IMultiple<Parsers.Thread> p) {
        _parser = p;
    }

    public async Task OnGet(int p) {
        HtmlNode rootNode = await Net.getNodeFromPage("forums", p);
        Page = p;
        Threads = _parser.ParseSingle(rootNode);
    }
}