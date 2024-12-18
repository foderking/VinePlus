using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Search.Posts;

[IgnoreAntiforgeryToken(Order = 1001)]
public class Index : PageModel
{
    public void OnGet() {
        
    }
    public IActionResult OnPost(string creator, string searchQuery) {
        return Redirect($"/search/posts/results?searchPost=true&query={searchQuery.Trim()}&creator={creator.Trim()}");
    }
}
