using System.Diagnostics;
using HtmlAgilityPack;
using WebAPI.Models;

namespace WebAPI.Repository.Parsers;

public class UserParserException : Exception
{
    public UserParserException(string message) : base($"Parsing Error in {message}") {
    }
    
    public UserParserException(string message, Exception inner) : base(message, inner) {
    }
}

public static class ProfileParser
{
    public static string CleanUsername(string input) {
        return input.ToLower().Replace(" ", "_");
    }

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

    /// <summary>
    /// Gets the first child node at path `div#js-kubrick-lead>div.profile-header>div.container` from the wrapperNode
    /// </summary>
    /// <param name="wrapperNode">the wrapperNode</param>
    /// <returns></returns>
    public static HtmlNode GetProfileHeaderContainer(HtmlNode wrapperNode) {
        return wrapperNode
            .FirstDirectDescendant(
                "div",
                div => string.Equals(
                    div.GetAttributeValue("id", ""),
                    "js-kubrick-lead"
                )
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("profile-header")
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("container")
            );
    }

    /// <summary>
    /// Gets the first child node at path `div.profile-title-hold>section.profile-follow>table` from the profileHeaderNode
    /// </summary>
    /// <param name="profileHeaderNode"></param>
    /// <returns></returns>
    public static HtmlNode GetStatsNode(HtmlNode profileHeaderNode) {
        return profileHeaderNode
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("profile-title-hold")
            )
            .FirstDirectDescendant(
                "section",
                section => section.HasClass("profile-follow")
            )
            .FirstDirectDescendant("table");
    }

    /// <summary>
    /// Gets the first child node at path `div.profile-title-hold>section.profile-title` from the profileHeaderNode
    /// </summary>
    /// <param name="profileHeaderNode"></param>
    /// <returns></returns>
    public static HtmlNode GetProfileTitleNode(HtmlNode profileHeaderNode) {
        return profileHeaderNode
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("profile-title-hold")
            )
            .FirstDirectDescendant(
                "section",
                section => section.HasClass("profile-title")
            );
    }

    /// <summary>
    /// Gets the first child node at path `div#site>div#default-content>aside` from the wrapperNode
    /// </summary>
    /// <param name="wrapperNode">wrapperNode</param>
    /// <returns></returns>
    /// <exception cref="UserParserException"></exception>
    public static HtmlNode GetAsideNode(HtmlNode wrapperNode) {
        return wrapperNode
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
 
    /// <summary>
    /// Gets first child node at path
    /// `div#site>div#default-content>div.primary-content.js-article-container>div.tab-content>div#js-user-main-feed>ul`
    /// from the wrapperNode
    /// </summary>
    /// <param name="wrapperNode"></param>
    /// <returns></returns>
    public static HtmlNode GetActivityListNode(HtmlNode wrapperNode) {
        return  wrapperNode
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
            .FirstDirectDescendant(
                "div",
                div => Equals(div.GetAttributeValue("id", ""), "js-user-main-feed")
            )
            .FirstDirectDescendant("ul");
    }
   
    
    public static string ParseUserName(HtmlNode profileTitleNode) {
        return profileTitleNode
            .DirectDescendants("h1")
            .Select(
                h1 => CleanUsername(h1.InnerText)
            )
            .First();
    }

    public static string ParseProfileTitle(HtmlNode profileTitleNode) {
        return profileTitleNode
            .FirstDirectDescendant(
                "h4",
                h4 => h4.HasClass("js-status-message")
            )
            .InnerText;
    }

    public static string ParseAvatarUrl(HtmlNode profileHeaderNode) {
        return profileHeaderNode
            .FirstDirectDescendant(
                "section",
                section => section.HasClass("profile-avatar")
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("avatar")
            )
            .DirectDescendants("img")
            .Select(
                img => img.GetAttributeValue("src", "")
            )
            .First();
    }

    public static string ParseForumPosts(HtmlNode profileStatsNode) {
        return profileStatsNode
            .FirstDirectDescendant(
                "tr"
            )
            .DirectDescendants("td")
            .Position(1)
            .InnerText;       
    }

    public static string ParseWikiPoints(HtmlNode profileStatsNode) {
        return profileStatsNode
            .FirstDirectDescendant(
                "tr"
            )
            .DirectDescendants("td")
            .Position(2)
            .InnerText;
    }

    public static Follow ParseFollowing(HtmlNode profileStatsNode) {
        return profileStatsNode
            .FirstDirectDescendant(
                "tr"
            )
            .DirectDescendants("td")
            .Position(3)
            .DirectDescendants("a")
            .Select(
                aNode => new Follow()
                {
                    Link = aNode.GetAttributeValue("href", null) ??
                           throw new UserParserException(nameof(ParseFollowers)),
                    Number = Convert.ToInt32(aNode.InnerText)
                }
            )
            .First();
    }

    public static Follow ParseFollowers(HtmlNode profileStatsNode) {
        return profileStatsNode
            .FirstDirectDescendant(
                "tr"
            )
            .DirectDescendants("td")
            .Position(4)
            .DirectDescendants("a")
            .Select(
                aNode => new Follow()
                {
                    Link = aNode.GetAttributeValue("href", null) ??
                           throw new UserParserException(nameof(ParseFollowers)),
                    Number = Convert.ToInt32(aNode.InnerText)
                }
            )
            .First();
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
                    div.ChildNodes
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
                int.Parse
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
                   div.ChildNodes
                       .Any(
                           each => each.Name=="div" && 
                                    each.HasClass("pod-body") && 
                                    each.HasClass("gallery-box-pod")
                       )
            );
    }
 
    // public static string[]? ParseLatestImages(HtmlNode asideNode) {
    //     HtmlNode? asidePod = GetLatestImageAsidePod(asideNode);
    //     if (asidePod == null) return null;
    //     return asidePod
    //         .FirstDirectDescendant(
    //             "div",
    //             div => div.HasClass("pod-body")
    //         )
    //         .DirectDescendants("figure")
    //         .Select(
    //             fig => fig
    //                 .Descendants("img")
    //                 .First()
    //                 .GetAttributeValue("src", "")
    //         )
    //         .ToArray();
    // }
   

    public static UserActivity ParseActivity(HtmlNode liActivityNode) {
        HtmlNode activityMessageNode = liActivityNode
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("media")
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("media-body")
            )
            .FirstDirectDescendant(
                "span",
                span => span.HasClass("activity-message")
            );
        
        // HtmlNode activityContentNode = mediaBodyNode
        //     .FirstDirectDescendant(
        //         "div",
        //         span => span.HasClass("activity-content")
        //     );
        UserActivity activity = new();
        // new DateTime( )
        return activity;
    }

    public static UserActivity[]? ParseUserActivities(HtmlNode wrapperNode) {
        HtmlNode activityNode = GetActivityListNode(wrapperNode);
        return activityNode
            .DirectDescendants("li")
            .Where(li => li.HasClass("activity-list__item"))
            .Select(ParseActivity)
            .ToArray();
    }
    
    public static Profile Parse<T>(HtmlNode rootNode, ILogger<T> logger) {
        Stopwatch timer = Stopwatch.StartNew();
        
        HtmlNode wrapperNode = GetWrapperNode(rootNode);
        
        HtmlNode asideNode = GetAsideNode(wrapperNode);
        
        HtmlNode profileHeaderNode = GetProfileHeaderContainer(wrapperNode);
        HtmlNode profileStatsNode = GetStatsNode(profileHeaderNode);
        HtmlNode profileTitleNode = GetProfileTitleNode(profileHeaderNode);
        

         Profile profile = new Profile();
         profile.Activities   = ParseUserActivities(wrapperNode);
         profile.AvatarUrl    = ParseAvatarUrl(profileHeaderNode);
         
         profile.UserName = ParseUserName(profileTitleNode);
         profile.ProfileDescription = ParseProfileTitle(profileTitleNode);
         
         profile.ForumPosts = Convert.ToInt32(ParseForumPosts(profileStatsNode));
         profile.WikiPoints = Convert.ToInt32(ParseWikiPoints(profileStatsNode));
         profile.Following  = ParseFollowing(profileStatsNode);
         profile.Followers  = ParseFollowers(profileStatsNode);
         
         profile.CoverPicture = ParseCoverPicture(asideNode);
         profile.AboutMe      = ParseAboutMe(asideNode);
         // profile.LatestImages = ParseLatestImages(asideNode);
         
         timer.Stop();
         logger.LogInformation("ProfileParser completed in {Elapsed}", Repository.GetElapsed(timer.Elapsed));
         return profile;
    }
}