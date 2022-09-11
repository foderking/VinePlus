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

    
    /// <summary>
    /// asideNode is the first html>body>div#site-main>div#wrapper>div#site>div#default-content>aside node
    /// </summary>
    /// <param name="rootNode">rootNode</param>
    /// <returns>asideNode</returns>
    /// <exception cref="UserParserException"></exception>
    public static HtmlNode GetAsideNode(HtmlNode rootNode) {
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
            )
            .FirstDirectDescendant(
                "div",
                div => string.Equals(
                    div.GetAttributeValue("id", ""), 
                    "site", StringComparison.Ordinal
                    )
            )
            .FirstDirectDescendant(
                "div",
                div => string.Equals(
                    div.GetAttributeValue("id", ""), 
                    "default-content", StringComparison.Ordinal
                    )
            )
            .DirectDescendants("aside")
            .FirstOrDefault() ?? throw new UserParserException(nameof(GetAsideNode));
    }

    public static string? ParseCoverPicture(HtmlNode asideNode) {
        HtmlNode? profileNode = asideNode
            .DirectDescendants("div")
            .FirstOrDefault(
                div => div.HasClass("profile-image")
            );
        if (profileNode == null) return null;
        return profileNode
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("img")
            )
            .FirstDirectDescendant(
                "a",
                a => a.HasClass("imgflare") || a.HasClass("imgclass")
            )
            .DirectDescendants("img")
            .Select(
                img => img.GetAttributeValue("src", "")
            )
            .FirstOrDefault();
    }
    
    /// <summary>
    /// asidePod is the first node with a child div.aside-pod which has a child header with text "About Me"
    /// </summary>
    /// <param name="asideNode">the asideNode</param>
    /// <returns>asidePod</returns>
    public static HtmlNode GetAboutMeAsidePod(HtmlNode asideNode) {
        return asideNode
            .FirstDirectDescendant(
                "div",
                div => 
                    div
                        .HasClass("aside-pod") && 
                   ((IEnumerable<HtmlNode>) div.ChildNodes)
                       .Any(
                           each => each.Name=="header" && Equals(each.InnerText.Trim(), "About Me")
                       )
            );
    }
    
    public static string? ParseSummaryText(HtmlNode asidePod) {
        HtmlNode? about = asidePod
            .DirectDescendants("div")
            .FirstOrDefault(
                div => div.HasClass("about-me")
            );
        if (about == null) return null;
        return about
            .FirstDirectDescendant("article")
            .InnerHtml
            .Trim();
    }

    public static DateTime ParseDateJoined(HtmlNode asidePod) {
        return asidePod
            .FirstDirectDescendant("ul")
            .DirectDescendants("li")
            .Select(
                li => li.InnerText.Split(":")
            )
            .Where(
                arr => arr[0].Equals("Date joined")
            )
            .Select(
                text => DateTime.Parse(text[1])
            )
            .First();
    }
 
    public static Alignment ParseAlignment(HtmlNode asidePod) {
        string align = asidePod
            .FirstDirectDescendant("ul")
            .DirectDescendants("li")
            .Select(
                li => li.InnerText.Split(":")
            )
            .Where(
                arr => arr[0].Equals("Alignment")
            )
            .Select(
                arr => arr[1].Split(" ")[0].Trim()
            )
            .First();
        
        // if (align == null) throw new UserParserException(nameof(ParseAlignment));
        return align switch
        {
            "Good"    => Alignment.Good,
            "Evil"    => Alignment.Evil,
            "Neutral" => Alignment.Neutral,
            "None"    => Alignment.None,
            _ => throw new UserParserException(nameof(ParseAlignment))
        };
    }
   
    public static int ParsePoints(HtmlNode asidePod) {
        return asidePod
            .FirstDirectDescendant("ul")
            .DirectDescendants("li")
            .Select(
                li => li.InnerText.Split(":")
            )
            .Where(
                arr => arr[0].Equals("Points")
            )
            .Select(
                arr => arr[1].Split(" ")[0].Trim()
            )
            .Select(
                text => int.Parse(text)
            )
            .First();
    }

    public static About ParseAboutMe(HtmlNode asideNode) {
        HtmlNode asidePod = GetAboutMeAsidePod(asideNode);
        // var xx = asidePod.DirectDescendants("div").ToArray();
        About about = new();
        about.Summary    = ParseSummaryText(asidePod);
        about.DateJoined = ParseDateJoined(asidePod);
        about.Points     = ParsePoints(asidePod);
        about.Alignment = ParseAlignment(asidePod);
        return about;
    }
    
    /// <summary>
    /// </summary>
    /// <param name="asideNode">the asideNode</param>
    /// <returns>asidePod</returns>
    public static HtmlNode? GetLatestImageAsidePod(HtmlNode asideNode) {
        return asideNode
            .FirstDirectDescendantOrDefault(
                "div",
                div => 
                    div
                        .HasClass("aside-pod") && 
                   ((IEnumerable<HtmlNode>) div.ChildNodes)
                       .Any(
                           each => each.Name=="div" && 
                                    each.HasClass("pod-body") && 
                                    each.HasClass("gallery-box-pod")
                       )
            );
    }
 
    public static string[]? ParseLatestImages(HtmlNode asideNode) {
        HtmlNode? asidePod = GetLatestImageAsidePod(asideNode);
        if (asidePod == null) return null;
        return asidePod
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("pod-body")
            )
            .DirectDescendants("figure")
            .Select(
                fig => fig
                    .Descendants("img")
                    .First()
                    .GetAttributeValue("src", "")
            )
            .ToArray();
    }

    public static HtmlNode? GetActivityListNode(HtmlNode rootNode) {
        HtmlNode? mainFeed = rootNode
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
            )
            .FirstDirectDescendant(
                "div",
                div => string.Equals(
                    div.GetAttributeValue("id", ""),
                    "site", StringComparison.Ordinal
                )
            )
            .FirstDirectDescendant(
                "div",
                div => string.Equals(
                    div.GetAttributeValue("id", ""),
                    "default-content", StringComparison.Ordinal
                )
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("primary-content") || div.HasClass("js-article-container")
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("tab-content")
            )
            .FirstDirectDescendantOrDefault(
                "div",
                div => Equals(div.GetAttributeValue("id", ""), "js-user-main-feed")
            );
        if (mainFeed == null) return null;
        return mainFeed
            .FirstDirectDescendantOrDefault("ul");
    }
    

    public static UserActivity ParseActivity(HtmlNode liActivityNode) {
        HtmlNode mediaBodyNode = liActivityNode
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("media")
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("media-body")
            );
        HtmlNode activityMessageNode = mediaBodyNode
            .FirstDirectDescendant(
                "span",
                span => span.HasClass("activity-message")
            );
        HtmlNode activityContentNode = mediaBodyNode
            .FirstDirectDescendant(
                "div",
                span => span.HasClass("activity-content")
            );
        UserActivity activity = new();
        // new DateTime( )
        return activity;
    }

    public static UserActivity[]? ParseUserActivities(HtmlNode rootNode) {
        HtmlNode? activityNode = GetActivityListNode(rootNode);
        if (activityNode == null) return null;
        return activityNode
            .DirectDescendants("li")
            .Select(ParseActivity)
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

         HtmlNode asideNode = GetAsideNode(rootNode);

         User user = new User();
         user.UserName = UserParser.ParseUserName(profileTitleNode);
         user.ProfileDescription = UserParser.ParseProfileTitle(profileTitleNode);
         user.AvatarUrl = UserParser.ParseAvatarUrl(profileAvatarNode);
         user.ForumPosts = Convert.ToInt32(UserParser.ParseForumPosts(profileStats));
         user.WikiPoints = Convert.ToInt32(UserParser.ParseWikiPoints(profileStats));
         user.Following = UserParser.ParseFollowing(profileStats);
         user.Followers = UserParser.ParseFollowers(profileStats);
         
         user.CoverPicture = UserParser.ParseCoverPicture(asideNode);
         user.AboutMe      = UserParser.ParseAboutMe(asideNode);
         user.LatestImages = UserParser.ParseLatestImages(asideNode);
         user.Activities   = UserParser.ParseUserActivities(rootNode);
         
         timer.Stop();
         logger.LogInformation($"UserParser completed in {Repository.GetElapased(timer.Elapsed)}");
         return user;
    }
}