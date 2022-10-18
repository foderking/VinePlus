using HtmlAgilityPack;
using WebAPI.Models;

namespace WebAPI.Repository.Parsers;

public class FollowersParserException : Exception
{
    public FollowersParserException(string message) : base($"Parsing Error in {message}") {
    }
}

public static class FollowersParser
{
    public static Followers ParseFollower(HtmlNode trNode) {
        HtmlNode tdNode = trNode.FirstDirectDescendant("td");

        HtmlNode aNode = tdNode.FirstDirectDescendant("a");
        HtmlNode imgNode = aNode.FirstDirectDescendant("img");
        
        Followers follower = new();
        follower.MiniAvatarUrl = imgNode.GetAttributeValue("src", null);
        follower.Text = aNode.InnerText.Trim();
        follower.Url = aNode.GetAttributeValue("href", null);
        
        return follower;
    }

    public static Followers[] ParseFollowers(HtmlNode mainNode) {
        return mainNode
            .FirstDirectDescendant("table")
            .FirstDirectDescendant("tbody")
            .DirectDescendants("tr")
            .Select(ParseFollower)
            .ToArray();
    }

    public static FollowersPage Parse<T>(HtmlNode rootNode, int pageNo, ILogger<T> logger) {
        HtmlNode wrapperNode = MainParser.GetWrapperNode(rootNode);
        HtmlNode mainNode = FollowingParser.GetMainNode(wrapperNode);

        HtmlNode? navNode = mainNode
            .FirstDirectDescendant("div", div => div.HasClass("navigation"))
            .FirstDirectDescendantOrDefault("ul");
        
        
        FollowersPage followerPage = new();
        followerPage.PageNo = pageNo;
        followerPage.Followers = ParseFollowers(mainNode);

        if (navNode == null)
            followerPage.TotalPages = 1;
        else {
            followerPage.TotalPages = Int32.Parse(
                navNode
                    .DirectDescendants(
                        "li", 
                        li => !li.HasClass("skip")
                    )
                    .Last()
                    .InnerText
            );
        }
        
        return followerPage;

    }
}