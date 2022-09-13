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
        // return profileAvatarNode
        //     .Select(
        //         node => node
        //             .Descendants("div")
        //             .First(
        //                 div => div
        //                     .HasClass("avatar")
        //             )
        //     )
        //     .Select(
        //         avatar => avatar
        //             .Descendants("img")
        //             .First()
        //             .GetAttributeValue("src", "")
        //     )
        //     .FirstOrDefault() ?? throw new UserParserException(nameof(ParseAvatarUrl));
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

        // return profileStats
        //     .Select(
        //         tbody => tbody
        //             .Descendants("tr")
        //             .First()
        //             .Descendants("td")
        //             .Skip(1)
        //             .First()
        //             .InnerHtml
        //     )
        //     .FirstOrDefault() ?? throw new UserParserException(nameof(ParseWikiPoints));
    }

    // public static Follow SelectFollow(HtmlNode tdNode) {
    //     HtmlNode aNode = tdNode
    //     string link = aNode.GetAttributeValue("href", null) ?? throw new  UserParserException(nameof(SelectFollow));
    //     int number = Convert.ToInt32(aNode.InnerHtml);
    //     return new Follow() { Link = link, Number = number };
    // }

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
        
        // return profileStats
        //     .Select(
        //         tbody => tbody
        //             .Descendants("tr")
        //             .First()
        //             .Descendants("td")
        //             .Skip(2)
        //             .First()
        //     )
        //     .Select(SelectFollow)
        //     .FirstOrDefault() ?? throw new UserParserException(nameof(ParseFollowing));
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
        // return profileStats
        //     
        //     .Select(
        //         table => table
        //             .Descendants("tr")
        //             .First()
        //             .Descendants("td")
        //             .Skip(3)
        //             .First()
        //     )
        //     .Select(SelectFollow)
        //     .FirstOrDefault() ?? throw new UserParserException(nameof(ParseFollowers));
    }

    
    /// <summary>
    /// asideNode is the first html>body>div#site-main>div#wrapper>div#site>div#default-content>aside node
    /// </summary>
    /// <param name="wrapperNode">rootNode</param>
    /// <returns>asideNode</returns>
    /// <exception cref="UserParserException"></exception>
    public static HtmlNode GetAsideNode(HtmlNode wrapperNode) {
        return wrapperNode
        // return rootNode
        //     .FirstDirectDescendant("html")
        //     .FirstDirectDescendant("body")
        //     .FirstDirectDescendant(
        //         "div",
        //         div => string.Equals(
        //                     div.GetAttributeValue("id", ""), 
        //                     "site-main", StringComparison.Ordinal
        //                 )
        //     )
        //     .FirstDirectDescendant(
        //         "div",
        //         div => string.Equals(
        //                     div.GetAttributeValue("id", ""), 
        //                     "wrapper", StringComparison.Ordinal
        //                 )
        //     )
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

    public static HtmlNode GetActivityListNode(HtmlNode wrapperNode) {
        return  wrapperNode
        // HtmlNode? mainFeed = rootNode
            // .FirstDirectDescendant("html")
            // .FirstDirectDescendant("body")
            // .FirstDirectDescendant(
            //     "div",
            //     div => string.Equals(
            //         div.GetAttributeValue("id", ""),
            //         "site-main", StringComparison.Ordinal
            //     )
            // )
            // .FirstDirectDescendant(
            //     "div",
            //     div => string.Equals(
            //         div.GetAttributeValue("id", ""),
            //         "wrapper", StringComparison.Ordinal
            //     )
            // )
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
            )//;
        // if (mainFeed == null) return null;
        // return mainFeed
            .FirstDirectDescendant("ul");
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

    public static UserActivity[]? ParseUserActivities(HtmlNode wrapperNode) {
        HtmlNode activityNode = GetActivityListNode(wrapperNode);
        // if (activityNode == null) return null;
        return activityNode
            .DirectDescendants("li")
            .Where(li => li.HasClass("activity-list__item"))
            .Select(ParseActivity)
            .ToArray();
    }
    
    public static User Parse<T>(HtmlNode rootNode, ILogger<T> logger) {
        Stopwatch timer = Stopwatch.StartNew();
        
        // IEnumerable<HtmlNode> sectionNode = rootNode
        //     .Descendants("section")
        //     .ToArray();

        // IEnumerable<HtmlNode> profileTitleNode = sectionNode
        //     .Where(
        //         node => node
        //             .HasClass("profile-title")
        //     )
        //     .ToArray();

        // IEnumerable<HtmlNode> profileFollowNode = sectionNode
        //     .Where(
        //         node => node
        //             .HasClass("profile-follow")
        //     )
        //     .ToArray();

        // IEnumerable<HtmlNode> profileStats = profileFollowNode
        //     .Select(
        //         node => node
        //             .Descendants("table")
        //             .First()
        //     )
        //     .ToArray();
            
         // IEnumerable<HtmlNode> profileAvatarNode = sectionNode
         //    .Where(
         //        node => node
         //            .HasClass("profile-avatar")
         //    )
         //    .ToArray();

        HtmlNode wrapperNode = GetWrapperNode(rootNode);
        
        HtmlNode asideNode = GetAsideNode(wrapperNode);
        
        HtmlNode profileHeaderNode = GetProfileHeaderContainer(wrapperNode);
        HtmlNode profileStatsNode = GetStatsNode(profileHeaderNode);
        HtmlNode profileTitleNode = GetProfileTitleNode(profileHeaderNode);
        

         User user = new User();
         user.Activities   = UserParser.ParseUserActivities(wrapperNode);
         user.AvatarUrl    = UserParser.ParseAvatarUrl(profileHeaderNode);
         
         user.UserName = UserParser.ParseUserName(profileTitleNode);
         user.ProfileDescription = UserParser.ParseProfileTitle(profileTitleNode);
         
         user.ForumPosts = Convert.ToInt32(UserParser.ParseForumPosts(profileStatsNode));
         user.WikiPoints = Convert.ToInt32(UserParser.ParseWikiPoints(profileStatsNode));
         user.Following  = UserParser.ParseFollowing(profileStatsNode);
         user.Followers  = UserParser.ParseFollowers(profileStatsNode);
         
         user.CoverPicture = UserParser.ParseCoverPicture(asideNode);
         user.AboutMe      = UserParser.ParseAboutMe(asideNode);
         user.LatestImages = UserParser.ParseLatestImages(asideNode);
         
         timer.Stop();
         logger.LogInformation($"UserParser completed in {Repository.GetElapased(timer.Elapsed)}");
         return user;
    }
}