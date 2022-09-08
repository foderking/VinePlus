using System.Collections.Specialized;
using System.Text;
using System.Web;
using HtmlAgilityPack;

namespace WebAPI.Repository;

public static class Repository
{
    public static HtmlNode GetRootNode(Stream htmlStream) {
        HtmlDocument htmlDoc = new ();
        htmlDoc.Load(htmlStream, true);
        Console.WriteLine(htmlDoc.Encoding);
        return htmlDoc.DocumentNode;
    }

    public static Task<Stream> GetStream(string path, Dictionary<string, string>? query = null) {
        NameValueCollection q = HttpUtility.ParseQueryString(string.Empty);
        if (query != null) {
            foreach (var (a,b) in query) {
                query[a] = b;
            }
        }
        HttpClient client = new HttpClient();
        UriBuilder uri = new("https://comicvine.gamespot.com");
        client.BaseAddress = new Uri("https://comicvine.gamespot.com");
        return client.GetStreamAsync($"{path}/{q}");
    }
}