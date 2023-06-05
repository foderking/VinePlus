using Comicvine.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComicVine.API.Pages.Profile;

public class Images : Navigator<ImgData>
{
    public string UserName = "";
    public Parsers.Image? Img;
    
    public async Task OnGet(string user, int p) {
        var profile = await Parsers.ProfileParser.ParseDefault($"/profile/{user}");
        Img = await Parsers.ImageParser.ParseDefault($"/profile/{user}/images");
        Entities = await Util.Image.GetImages(Img,p-1);
        UserName = profile.UserName;
        NavRecord = new(p, Int32.MaxValue, UserName);
    }

    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/profile/images/{user}/{page}";
    }
}