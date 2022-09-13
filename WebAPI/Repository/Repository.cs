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
    // public static IEnumerable<HtmlNode> DirectDescendants(this HtmlNode mainNode) {
    // }
    /// <summary>
    /// Gets all direct child nodes of the current node
    /// </summary>
    /// <param name="mainNode">The current node</param>
    /// <param name="name">The type of all child nodes to return</param>
    /// <returns>An enumerable representing all valid child nodes</returns>
    public static IEnumerable<HtmlNode> DirectDescendants(this HtmlNode mainNode, string name) {
        return mainNode
            .Elements(name);
    }
    
    public static IEnumerable<HtmlNode> DirectDescendants(this HtmlNode mainNode, string name, Func<HtmlNode, bool> predicate) {
        return mainNode
            .Elements(name)
            .Where(predicate);
    }
    
    /// <summary>
    /// Gets the first direct child nodes of the current node
    /// </summary>
    /// <param name="mainNode">The current node</param>
    /// <param name="name">The type of the child node</param>
    /// <returns>The first valid child node</returns>
    public static HtmlNode FirstDirectDescendant(this HtmlNode mainNode, string name) {
        return mainNode
            .Elements(name)
            .First();
    }
    
    public static HtmlNode FirstDirectDescendant(this HtmlNode mainNode, string name, Func<HtmlNode, bool> predicate) {
        return mainNode
            .Elements(name)
            .First(predicate);
    }
    
    public static HtmlNode? FirstDirectDescendantOrDefault(this HtmlNode mainNode, string name, Func<HtmlNode, bool> predicate) {
        return mainNode
            .Elements(name)
            .FirstOrDefault(predicate);
    }
    
    public static HtmlNode? FirstDirectDescendantOrDefault(this HtmlNode mainNode, string name) {
        return mainNode
            .Elements(name)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets element at specified position (position starts from 1)
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="position"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Position<T>(this IEnumerable<T> collection, int position) {
        if (position < 1) throw new ArgumentException("Position should be greater than 1");
        return collection
            .Skip(position - 1)
            .First();
    }

}