﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Search;

public class Index : PageModel
{
    public IActionResult OnGet() {
        return Redirect("/search/threads");
    }
}