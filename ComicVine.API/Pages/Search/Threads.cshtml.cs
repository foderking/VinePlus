using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Search;

[IgnoreAntiforgeryToken(Order = 1001)] // disables anti-forgery token requirement   https://www.learnrazorpages.com/security/request-verification
public class SearchThreads : PageModel
{
    public void OnGet() {
    }
        
     public IActionResult OnPost(string? creator, string searchQuery) {
        return Redirect($"/search/results?searchPost=false&query={searchQuery}");
    }
}
