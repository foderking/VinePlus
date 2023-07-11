using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Search;

[IgnoreAntiforgeryToken(Order = 1001)] // disables anti-forgery token requirement   https://www.learnrazorpages.com/security/request-verification
public class Index : PageModel
{
    private ILogger<Index> _logger;
    public Index(ILogger<Index> _log) {
        _logger = _log;
    }
    public void OnGet() {
        _logger.LogInformation("get");
    }

     public IActionResult OnPost(string? creator, string searchQuery) {
        return Redirect($"/search/results?searchPost=false&query={searchQuery}");
    }
}