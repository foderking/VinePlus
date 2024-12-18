using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinePlus.Web.Pages;

public class Index : PageModel
{
    public IActionResult OnGet() {
        return Redirect("/forums");
    }
}