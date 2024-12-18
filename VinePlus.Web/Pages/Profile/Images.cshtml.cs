using Comicvine.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinePlus.Web.Pages.Profile;

public class Images : Navigator<ImgData>
{
    public string UserName = "";
    public Parsers.Image? Img;
    
    public async Task OnGet(string user, int p) {
        try
        {
            var profile = await Parsers.ProfileParser.ParseDefault($"/profile/{user}");
            Img = await Parsers.ImageParser.ParseDefault($"/profile/{user}/images");
            Entities = await Util.Image.GetImages(Img,p-1);
            UserName = profile.UserName;
            NavRecord = new(p, Int32.MaxValue, UserName);
        }
        catch
        {
            NavRecord = new(1, 1, UserName);
            /* needed for navigation to work for deactivated accounts */
            if (user.StartsWith("deactivated")) {
                UserName = user;
            }
        }
    }

    public override Func<string, int, string> PageDelegate() {
        return (user, page) => $"/profile/images/{user}/{page}";
    }
}