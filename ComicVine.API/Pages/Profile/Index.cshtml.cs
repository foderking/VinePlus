﻿using Comicvine.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Profile;

public class Index : PageModel
{
    public Parsers.Profile? UserProfile;
    public string UserName = "";

    public string FallBackImage =
        "https://comicvine.gamespot.com/a/uploads/original/11173/111735759/8970504-6560831187-Blank.jpg";
    
    public async Task OnGet(string user) {
        UserProfile = await Parsers.ProfileParser.ParseDefault($"/profile/{user}");
        UserName = UserProfile.UserName;
    }
}