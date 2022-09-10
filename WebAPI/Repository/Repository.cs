using System.Collections.Specialized;
using System.Text;
using System.Web;
using HtmlAgilityPack;

namespace WebAPI.Repository;

public static class Repository
{
    public static HtmlNode GetRootNode(Stream htmlStream) {
        HtmlDocument htmlDoc = new ();
        htmlDoc.Load(htmlStream);
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
    
    public static string GetElapased(TimeSpan timeSpan) {
        int netTime = (int) timeSpan.TotalSeconds;
        double milli = timeSpan.TotalSeconds - netTime;
        int hour = netTime / 3600;
        netTime %= 3600;
        int minute = netTime / 60;
        netTime %= 60;
        int seconds = netTime;
        // int approxTime = seconds  + minute * 60 + hour * 3600;
        // Console.WriteLine($"{timeSpan.TotalSeconds}, {approxTime}, {milli}");
        return $"{hour}h:{minute}m:{seconds}.{(int)(milli*1000)}";
    }

}