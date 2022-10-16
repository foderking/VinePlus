using System.Collections.Specialized;
using System.Text;
using System.Web;
using HtmlAgilityPack;

namespace WebAPI.Repository;

/// <summary>
/// A collections of useful helper functions for the repository
/// </summary>
public static class Repository
{
    /// <summary>
    /// Gets the root HTML document node from an HTML stream
    /// </summary>
    /// <param name="htmlStream">A stream representing an HTML</param>
    /// <returns>An HtmlNode representing the html root document</returns>
    public static HtmlNode GetRootNode(Stream htmlStream) {
        HtmlDocument htmlDoc = new ();
        htmlDoc.Load(htmlStream);
        return htmlDoc.DocumentNode;
    }

    /// <summary>
    /// Gets an HTML stream of the webpage on comicvine specified by a path, and an optional query
    /// </summary>
    /// <param name="path">The path to the webpage on comicvine</param>
    /// <param name="query">Optional queries to pass to the webpage</param>
    /// <returns>The HTML stream of the webpage</returns>
    public static Task<Stream> GetStream(string path, Dictionary<string, string>? query = null) {
        NameValueCollection q = HttpUtility.ParseQueryString(string.Empty);
        if (query != null)
            foreach (var (a,b) in query) {
                q[a] = b;
            }
        
        HttpClient client  = new HttpClient();
        UriBuilder uri     = new("https://comicvine.gamespot.com");
        client.BaseAddress = new Uri("https://comicvine.gamespot.com");
        return client.GetStreamAsync($"{path}?{q}");
    }    
    
    /// <summary>
    /// Get the string representation of a timespan
    /// </summary>
    /// <param name="timeSpan">The specified timespan</param>
    /// <returns>The string representation of the timespan</returns>
    public static string GetElapsed(TimeSpan timeSpan) {
        int netTime = (int) timeSpan.TotalSeconds;
        double milli = timeSpan.TotalSeconds - netTime;
        int hour = netTime / 3600;
        netTime %= 3600;
        int minute = netTime / 60;
        netTime %= 60;
        int seconds = netTime;
        return $"{hour}h:{minute}m:{seconds}.{(int)(milli*1000)}";
    }
    
    /// <summary>
    /// Gets all direct child nodes of a specified node that has specific name
    /// </summary>
    /// <param name="mainNode">The specified node</param>
    /// <param name="name">The name/type of all child nodes to return</param>
    /// <returns>An enumerable representing all valid child nodes</returns>
    public static IEnumerable<HtmlNode> DirectDescendants(this HtmlNode mainNode, string name) {
        return mainNode
            .Elements(name);
    }
    
    /// <summary>
    ///  Gets all direct child nodes of a specified node that has specific name, and satisfies a specific condition
    /// </summary>
    /// <param name="mainNode">The specified node</param>
    /// <param name="name">The name/type of all child nodes to return</param>
    /// <param name="predicate">An extra condition the child nodes must satisfy</param>
    /// <returns>An enumerable representing all valid child nodes</returns>
    public static IEnumerable<HtmlNode> DirectDescendants(this HtmlNode mainNode, string name, Func<HtmlNode, bool> predicate) {
        return mainNode
            .Elements(name)
            .Where(predicate);
    }
    
    /// <summary>
    /// Gets the first direct child nodes of a specified node with a specific name
    /// </summary>
    /// <param name="mainNode">The specified node</param>
    /// <param name="name">The name/type of the child node</param>
    /// <returns>The first valid child node</returns>
    public static HtmlNode FirstDirectDescendant(this HtmlNode mainNode, string name) {
        return mainNode
            .Elements(name)
            .First();
    }
    
    /// <summary>
    ///  Gets the first direct child nodes of a specified node with a specific name, and satisfies a specific condition
    /// </summary>
    /// <param name="mainNode">The specified node</param>
    /// <param name="name">The name/type of all child nodes to return</param>
    /// <param name="predicate">An extra condition the child nodes must satisfy</param>
    /// <returns>The first valid child node</returns>
    public static HtmlNode FirstDirectDescendant(this HtmlNode mainNode, string name, Func<HtmlNode, bool> predicate) {
        return mainNode
            .Elements(name)
            .First(predicate);
    }

    /// <summary>
    /// Gets the first direct child nodes of a specified node with a specific name
    /// Returns a default value if a valid node isn't found
    /// </summary>
    /// <param name="mainNode">The specified node</param>
    /// <param name="name">The name/type of the child node</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The first valid child node</returns>
    public static HtmlNode? FirstDirectDescendantOrDefault(this HtmlNode mainNode, string name, HtmlNode? defaultValue = null) {
        return mainNode
            .Elements(name)
            .FirstOrDefault(defaultValue);
    }
    
    /// <summary>
    ///  Gets the first direct child nodes of a specified node with a specific name, and satisfies a specific condition.
    ///  Returns a default value if a valid node isn't found
    /// </summary>
    /// <param name="mainNode">The specified node</param>
    /// <param name="name">The name/type of all child nodes to return</param>
    /// <param name="predicate">An extra condition the child nodes must satisfy</param>
    /// <returns>The first valid child node</returns>      
    public static HtmlNode? FirstDirectDescendantOrDefault(this HtmlNode mainNode, string name, Func<HtmlNode, bool> predicate) {
        return mainNode
            .Elements(name)
            .FirstOrDefault(predicate);
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