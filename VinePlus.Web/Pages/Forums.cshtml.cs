using Comicvine.Core;
using HtmlAgilityPack;

namespace VinePlus.Web.Pages;

public class Forums : Pagination<ThreadView>, IForum
{
    public async Task OnGet(int p) {
        HtmlNode rootNode = await Net.getNodeFromPage("/forums", p);
        Entities = Parsers.ThreadParser.ParseSingle(rootNode).Select(thread => ThreadView.create(thread));
        int last = Parsers.Common.parsePageEnd(rootNode);
        NavRecord = new(p, last, "");
    }

    public override Func<string, int, string> PageDelegate() {
        return (_, page) => $"/forums/{page}";
    }

    public Func<ThreadView, string> GetThreadLink() {
        return (thread) => $"/thread?path={thread.thread_link}";
    }
}