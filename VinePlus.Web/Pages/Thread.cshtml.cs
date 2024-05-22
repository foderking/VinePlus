using Comicvine.Core;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages;

public class Thread : Navigator<Parsers.Post>
{
    public string ThreadTitle = "";
    public string OriginalThread = "";

    public async Task OnGet(string path, int p=1) {;
        HtmlNode node = await Net.getNodeFromPage(path, p);
        Entities = Parsers.PostParser.ParseSingle(node);
        ThreadTitle = Parsers.Common.getThreadTitle(node);
        int last = Parsers.PostParser.ParseEnd(node);
        NavRecord = new Nav(DelegateParam: path, CurrentPage: p, LastPage: last);
        OriginalThread = path;
    }

    public override Func<string, int, string> PageDelegate() {
        return (path, page) => $"/thread?path={path}&p={page}";
    }
}