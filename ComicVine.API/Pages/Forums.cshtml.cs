using Comicvine.Core;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages;

public class Forums : PageModel
{
    private Parsers.IMultiple<Parsers.Thread> _parser;
    public IEnumerable<Parsers.Thread>? Threads;

    public Forums(Parsers.IMultiple<Parsers.Thread> p) {
        _parser = p;
    }
    public async Task OnGet(int p) {
        // Stream stream = await Net.getStreamByPage(p, "forums");
        HtmlNode rootNode = await Net.getNodeFromPage("forums", p);
        Threads = _parser.ParseSingle(rootNode);
    }
}