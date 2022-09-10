using System.Diagnostics;
using HtmlAgilityPack;
using WebAPI.Models;

namespace WebAPI.Repository.Parsers;

public class UserParserException : Exception
{
    public UserParserException(): base() {
    }
        
    public UserParserException(string message) : base($"Parsing Error in {message}") {
    }
    
    public UserParserException(string message, Exception inner) : base(message, inner) {
    }
}

public static class UserParser
{
    public static string CleanUsername(string input) {
        return input.ToLower().Replace(" ", "_");
    }

    public static string ParseUserName(IEnumerable<HtmlNode> profileTitleNode) {
        return profileTitleNode
            .Select(
                node => node
                    .Descendants("h1")
                    .First()
                    .InnerHtml
            )
            .Select(CleanUsername)
            .FirstOrDefault() ?? throw new UserParserException(nameof(ParseUserName));
    }

    public static string ParseProfileTitle(IEnumerable<HtmlNode> profileTitleNode) {
        return profileTitleNode
            .Select(
                node => node
                    .Descendants("h4")
                    .First(
                        h4 => h4
                            .HasClass("js-status-message")
                    )
                    .InnerHtml
            )
            .FirstOrDefault() ?? throw new UserParserException(nameof(ParseProfileTitle));
    }

    public static string ParseAvatarUrl(IEnumerable<HtmlNode> profileAvatarNode) {
        return profileAvatarNode
            .Select(
                node => node
                    .Descendants("div")
                    .First(
                        div => div
                            .HasClass("avatar")
                    )
            )
            .Select(
                avatar => avatar
                    .Descendants("img")
                    .First()
                    .GetAttributeValue("src", "")
            )
            .FirstOrDefault() ?? throw new UserParserException(nameof(ParseAvatarUrl));
    }

    public static string ParseForumPosts(IEnumerable<HtmlNode> profileStats) {
        return profileStats
            .Select(
                tbody => tbody
                    .Descendants("tr")
                    .First()
                    .Descendants("td")
                    .Skip(0)
                    .First()
                    .InnerHtml
            )
            .FirstOrDefault() ?? throw new UserParserException(nameof(ParseForumPosts));
    }

    public static string ParseWikiPoints(IEnumerable<HtmlNode> profileStats) {
        return profileStats
            .Select(
                tbody => tbody
                    .Descendants("tr")
                    .First()
                    .Descendants("td")
                    .Skip(1)
                    .First()
                    .InnerHtml
            )
            .FirstOrDefault() ?? throw new UserParserException(nameof(ParseWikiPoints));
    }

    public static Follow SelectFollow(HtmlNode tdNode) {
        HtmlNode aNode = tdNode.Descendants("a").First();
        string link = aNode.GetAttributeValue("href", null) ?? throw new  Exception("error parsing follow");
        int number = Convert.ToInt32(aNode.InnerHtml);
        return new Follow() { Link = link, Number = number };
    }

    public static Follow ParseFollowing(IEnumerable<HtmlNode> profileStats) {
        return profileStats
            .Select(
                tbody => tbody
                    .Descendants("tr")
                    .First()
                    .Descendants("td")
                    .Skip(2)
                    .First()
            )
            .Select(SelectFollow)
            .FirstOrDefault() ?? throw new UserParserException(nameof(ParseFollowing));
    }

    public static Follow ParseFollowers(IEnumerable<HtmlNode> profileStats) {
        return profileStats
            .Select(
                tbody => tbody
                    .Descendants("tr")
                    .First()
                    .Descendants("td")
                    .Skip(3)
                    .First()
            )
            .Select(SelectFollow)
            .FirstOrDefault() ?? throw new UserParserException(nameof(ParseFollowers));
    }

    public static HtmlNode GetSecondaryContentNode(HtmlNode rootNode) {
        return rootNode
            .Descendants("div")
            .First(
                div => string.Equals(div.GetAttributeValue("id", ""), "site-main", StringComparison.Ordinal)
            )
            .Descendants("div")
            .First(
                div => string.Equals(div.GetAttributeValue("id", ""), "wrapper", StringComparison.Ordinal)
            )
            .Descendants("div")
            .First(
                div => string.Equals(div.GetAttributeValue("id", ""), "site", StringComparison.Ordinal)
            )
            .Descendants("div")
            .First(
                div => string.Equals(div.GetAttributeValue("id", ""), "default-content", StringComparison.Ordinal)
            )
            .Descendants("aside")
            .FirstOrDefault() ?? throw new UserParserException(nameof(GetSecondaryContentNode));
    }

    public static string? ParseCoverPicture(HtmlNode asideNode) {
        HtmlNode? imgNode = asideNode
            .Descendants("div")
            .FirstOrDefault(
                div => div.HasClass("profile-image")
            );
        if (imgNode == null) return null;
        return imgNode
            .Descendants("img")
            .Select(
                a => a.GetAttributeValue("src", "")
            )
            .FirstOrDefault();
    }

    public static string? ParseSummaryText(HtmlNode asidePos) {
        HtmlNode? about = asidePos
            .Descendants("div")
            .FirstOrDefault(
                div => div.HasClass("about-me")
            );
        if (about == null) return null;
        return about
            .Descendants("p")
            .Select(
                p => p.InnerHtml
            )
            .FirstOrDefault();
    }

    public static DateTime ParseDateJoined(HtmlNode asidePos) {
        return asidePos
            .Descendants("li")
            .Select(
                li => li.InnerText.Split(":")[1]
            )
            .Select(
                text => DateTime.Parse(text)
            )
            .First();
    }
    
    public static int ParsePoints(HtmlNode asidePos) {
        return asidePos
            .Descendants("li")
            .Skip(2)
            .Select(
                li => li.InnerText.Split(":")[1].Split(" ")[0].Trim()
            )
            .Select(
                text => int.Parse(text)
            )
            .First();
    }

    public static Alignment ParseAlignment(HtmlNode asidePos) {
        string? align = asidePos
            .Descendants("li")
            .Skip(1)
            .Select(
                li => li
                    .InnerText
                    .Split(":")[1]
                    .Split(" ")[0]
                    .Trim()
            )
            .FirstOrDefault();
        
        if (align == null) throw new UserParserException(nameof(ParseAlignment));
        
        return align switch
        {
            "Good"    => Alignment.Good,
            "Evil"    => Alignment.Evil,
            "Neutral" => Alignment.Neutral,
            "None"    => Alignment.None,
            _ => throw new UserParserException(nameof(ParseAlignment))
        };
    }
    public static About ParseAboutMe(HtmlNode asideNode) {
        HtmlNode asidePos = asideNode
            .Descendants("div")
            .First(
                div => div.HasClass("aside-pod")
            );
        About about = new();
        about.Summary    = ParseSummaryText(asidePos);
        about.DateJoined = ParseDateJoined(asidePos);
        about.Points     = ParsePoints(asidePos);
        about.Alignment = ParseAlignment(asidePos);
        return about;
    }

    public static string[] ParseLatestImages(HtmlNode asideNode) {
        return asideNode
            .Descendants("div")
            .Where(
                div => div.HasClass("aside-pod")
            )
            .Skip(1)
            .First(
            )
            .Descendants("figure")
            .Select(
                fig => fig
                    .Descendants("img")
                    .First()
                    .GetAttributeValue("src", "")
            )
            .ToArray();
    }
    
    public static User Parse<T>(HtmlNode rootNode, ILogger<T> logger) {
        Stopwatch timer = Stopwatch.StartNew();
        IEnumerable<HtmlNode> sectionNode = rootNode
            .Descendants("section")
            .ToArray();

        IEnumerable<HtmlNode> profileTitleNode = sectionNode
            .Where(
                node => node
                    .HasClass("profile-title")
            )
            .ToArray();

        IEnumerable<HtmlNode> profileFollowNode = sectionNode
            .Where(
                node => node
                    .HasClass("profile-follow")
            )
            .ToArray();

        IEnumerable<HtmlNode> profileStats = profileFollowNode
            .Select(
                node => node
                    .Descendants("table")
                    .First()
            )
            .ToArray();
            
         IEnumerable<HtmlNode> profileAvatarNode = sectionNode
            .Where(
                node => node
                    .HasClass("profile-avatar")
            )
            .ToArray();

         HtmlNode asideNode = GetSecondaryContentNode(rootNode);

         User user = new User();
         user.UserName = UserParser.ParseUserName(profileTitleNode);
         user.ProfileDescription = UserParser.ParseProfileTitle(profileTitleNode);
         user.AvatarUrl = UserParser.ParseAvatarUrl(profileAvatarNode);
         user.ForumPosts = Convert.ToInt32(UserParser.ParseForumPosts(profileStats));
         user.WikiPoints = Convert.ToInt32(UserParser.ParseWikiPoints(profileStats));
         user.Following = UserParser.ParseFollowing(profileStats);
         user.Followers = UserParser.ParseFollowers(profileStats);
         
         user.CoverPicture = UserParser.ParseCoverPicture(asideNode);
         user.AboutMe = UserParser.ParseAboutMe(asideNode);
         user.LatestImages = UserParser.ParseLatestImages(asideNode);
         
         timer.Stop();
         logger.LogInformation($"UserParser completed in {Repository.GetElapased(timer.Elapsed)}");
         return user;
    }
}