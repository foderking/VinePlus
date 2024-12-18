using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinePlus.Web.Pages.Search.Threads;

[IgnoreAntiforgeryToken(Order = 1001)] // disables anti-forgery token requirement   https://www.learnrazorpages.com/security/request-verification
public class Index : PageModel
{
    public void OnGet() {
    }
        
     public IActionResult OnPost(string? creator, string searchQuery) {
         return creator switch
         {
             null => 
                 Redirect($"/search/threads/results?searchPost=false&query={searchQuery.Trim()}&p=1"),
             _ =>
                 Redirect($"/search/threads/results?searchPost=false&query={searchQuery.Trim()}&creator={creator.Trim()}&p=1")
         };
    }
}
