using HtmlAgilityPack;
using WebAPI.Models;

namespace WebAPI.Repository.Parsers;

public class FollowingParserException : Exception
{
    public FollowingParserException(string message) : base($"Parsing Error in {message}") {
    }
}

public static class FollowingParser
{
    /// <summary>
    /// Getts the node at path
    /// `div#site>div#default-content>div.primary-content
    /// </summary>
    /// <param name="wrapperNode"></param>
    /// <returns></returns>
    public static HtmlNode GetMainNode(HtmlNode wrapperNode) {
        return wrapperNode
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("id", "") == "site"
            )
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("id", "") == "default-content"
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("primary-content")
            )
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("id", "") == "js-sort-filter-results"
            );
    }

    public static Following ParseFollowing(HtmlNode trNode) {
        HtmlNode[] tdNodes = trNode.DirectDescendants("td").ToArray();
        if (tdNodes.Length != 2) throw new FollowingParserException(nameof(ParseFollowing));

        HtmlNode aNode = tdNodes[0].FirstDirectDescendant("a");
        HtmlNode imgNode = aNode.FirstDirectDescendant("img");
        
        Following following = new();
        following.Type = tdNodes[1].InnerText.Trim();
        following.MiniAvatarUrl = imgNode.GetAttributeValue("src", null);
        following.Text = aNode.InnerText.Trim();
        following.Url = aNode.GetAttributeValue("href", null);
        
        return following;
    }

    public static Following[] ParseFollowings(HtmlNode mainNode) {
        return mainNode
            .FirstDirectDescendant("table")
            .FirstDirectDescendant("tbody")
            .DirectDescendants("tr")
            .Select(ParseFollowing)
            .ToArray();
    }

    public static FollowingPage Parse<T>(HtmlNode rootNode, int pageNo, ILogger<T> logger) {
        HtmlNode wrapperNode = MainParser.GetWrapperNode(rootNode);
        HtmlNode mainNode = GetMainNode(wrapperNode);

        HtmlNode? navNode = mainNode
            .FirstDirectDescendant("div", div => div.HasClass("navigation"))
            .FirstDirectDescendantOrDefault("ul");
        
        
        FollowingPage followingPage = new();
        followingPage.PageNo = pageNo;
        followingPage.Followings = ParseFollowings(mainNode);

        if (navNode == null)
            followingPage.TotalPages = 1;
        else {
            followingPage.TotalPages = Int32.Parse(
                navNode
                    .DirectDescendants(
                        "li", 
                        li => !li.HasClass("skip")
                    )
                    .Last()
                    .InnerText
            );
        }
        
        return followingPage;

    }
}