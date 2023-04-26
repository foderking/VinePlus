using Comicvine.Core;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages;

public class Forum : PageModel
{
    private Parsers.IMultiple<Parsers.Thread> _parser;
    public IEnumerable<Parsers.Thread>? Threads;

    public Forum(Parsers.IMultiple<Parsers.Thread> p) {
        _parser = p;
    }
    public async Task OnGet(int? pag) {
        int p = pag ?? 1;
        Stream stream = await Net.getStreamByPage(p, "forums");
        HtmlNode rootNode = Net.getRootNode(stream);
        Threads = _parser.ParseSingle(rootNode);
    }
}