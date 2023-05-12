using Comicvine.Core;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages;

public class Thread : PageModel
{
    private Parsers.IMultiple<Parsers.Post> _parser;
    public IEnumerable<Parsers.Post>? Posts;
    public string ThreadTitle = "";
    public Nav FNav = new (1, 1, "/forums");

    public Thread(Parsers.IMultiple<Parsers.Post> parser) {
        _parser = parser;
    }
    public async Task OnGet(string path, int p=1) {;
        HtmlNode node = await Net.getNodeFromPage(path, p);
        Posts = _parser.ParseSingle(node);//await Parsers.Common.ParseSingle(_parser, p, path);
        ThreadTitle = Parsers.Common.getThreadTitle(node);
        int last = Parsers.Common.parsePageEnd(node);
        FNav = new Nav(Path: path, CurrentPage: p, LastPage: last);
    }
}