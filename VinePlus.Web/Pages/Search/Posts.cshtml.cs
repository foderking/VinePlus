using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Search;

public class Posts : PageModel
{
    public void OnGet() {
        
    }
    public IActionResult OnPost(string creator, string searchQuery) {
        return Redirect($"/search/results?searchPost=true&query={searchQuery.Trim()}&creator={creator.Trim()}");
    }
}
