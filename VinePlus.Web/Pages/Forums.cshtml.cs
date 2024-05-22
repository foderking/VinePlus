using Comicvine.Core;
using HtmlAgilityPack;

namespace ComicVine.API.Pages;

public class Forums : Navigator<Parsers.Thread>, IForum
{
    public async Task OnGet(int p) {
        HtmlNode rootNode = await Net.getNodeFromPage("/forums", p);
        Entities = Parsers.ThreadParser.ParseSingle(rootNode);
        int last = Parsers.Common.parsePageEnd(rootNode);
        NavRecord = new(p, last, "");
    }

    public override Func<string, int, string> PageDelegate() {
        return (_, page) => $"/forums/{page}";
    }

    public Func<Parsers.Thread, string> GetThreadLink() {
        return (thread) => $"/thread?path={thread.Thread.Link}";
    }
}