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

    public string GetLock(bool isLocked) {
        return
            isLocked
            ? "<img src='https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-lock-16x16.png'>"
            : "";
    }
    
    public string GetPin(bool isPinned) {
        return
            isPinned
            ? "<img src='https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-pin-16x16.png'>"
            : "";
    }
    public async Task OnGet(int p) {
        HtmlNode rootNode = await Net.getNodeFromPage("forums", p);
        Threads = _parser.ParseSingle(rootNode);
    }
}