using System.Text.Json;
using Comicvine.Core;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace VinePlus.Web.Pages;

public static class Helpers
{
    public static string getDuration(DateTime current, DateTime date) {
        TimeSpan dur = current - date;
        return dur.Days switch
        {
            0 when dur is { Hours: 0, Minutes: 0, Seconds: 1 }=> 
                $"{dur.Seconds} second",
            0 when dur is { Hours: 0, Minutes: 0 } => 
                $"{dur.Seconds} seconds",
            0 when dur is { Hours: 0, Minutes: 1 } => 
                $"{dur.Minutes} minute",
            0 when dur.Hours == 0 => 
                $"{dur.Minutes} minutes",
            0 when dur.Hours == 1 => 
                $"{dur.Hours} hour",
            0 => 
                $"{dur.Hours} hours",
            1 => 
                $"{dur.Days} day",
            _ => 
                date.ToLongDateString()
        };
    }

    public static string getThreadRow(int index) {
        return index % 2  == 1? "thread-odd" : "";
    }

    public static string getHighlightClass(ViewDataDictionary view_data, string expected) {
        if (expected == (string)(view_data[Keys.Highlight] ?? "")) {
            return "nav-item nav-highlight";
        }
        return "nav-item";
    }

    public static string getPostClass(bool post_is_deleted) {
        return post_is_deleted ? "post-item post-deleted" : "post-item";
    }

    public static string getProfileLink(ViewDataDictionary view_data, string path) {
        string user = (string)(view_data[Keys.ProfileName] ?? "");
        return path + user;
    }

    public static async Task<IEnumerable<ImageData>> getImages(Parsers.Image data, int offset) {
        const int batch_size = 36;
        using HttpClient client = new();
        string response = await client.GetStringAsync(
            $"https://www.comicvine.gamespot.com/js/image-data.json?images={data.GalleryId}&object={data.ObjectId}&start={offset * batch_size}&count={batch_size}"
        );
        ImageResponse? res = JsonSerializer.Deserialize<ImageResponse>(response);
        return res!.images;
    }
}