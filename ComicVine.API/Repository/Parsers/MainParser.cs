using HtmlAgilityPack;

namespace ComicVine.API.Repository.Parsers;

public class MainParser
{
    /// <summary>
    /// Gets the first child node at path `html>body>div#site-main>div#wrapper` from the root document node
    /// </summary>
    /// <param name="rootNode">The root HTML document node</param>
    /// <returns></returns>
    public static HtmlNode GetWrapperNode(HtmlNode rootNode) {
        return rootNode
            .FirstDirectDescendant("html")
            .FirstDirectDescendant("body")
            .FirstDirectDescendant(
                "div",
                div => string.Equals(
                    div.GetAttributeValue("id", ""),
                    "site-main", StringComparison.Ordinal
                )
            )
            .FirstDirectDescendant(
                "div",
                div => string.Equals(
                    div.GetAttributeValue("id", ""),
                    "wrapper", StringComparison.Ordinal
                )
            );
    }
}