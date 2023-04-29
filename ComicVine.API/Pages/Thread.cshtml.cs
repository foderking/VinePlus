using Comicvine.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages;

public class Thread : PageModel
{
    private Parsers.IMultiple<Parsers.Post> _parser;
    public IEnumerable<Parsers.Post>? Posts;
    // public Parsers.Thread? Thread_;

    public int Page = 1;
    public int LastPage = Util.LastPage;
    public Thread(Parsers.IMultiple<Parsers.Post> parser) {
        _parser = parser;
    }
    public async Task OnGet(string path, int p) {
        Posts = await Parsers.Common.ParseSingle(_parser, p, path);
    }
}