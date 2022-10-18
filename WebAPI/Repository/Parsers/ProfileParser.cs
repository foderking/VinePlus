using System.Diagnostics;
using HtmlAgilityPack;
using WebAPI.Models;

namespace WebAPI.Repository.Parsers;

public class UserParserException : Exception
{
    public UserParserException(string message) : base($"Parsing Error in {message}") {
    }
}

public static class ProfileParser
{
    public static string CleanUsername(string input) {
        return input.ToLower().Replace(" ", "_");
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
   
    
    /// <summary>
    /// Parses the username: which is at path
    /// `h1`
    /// from the profileTitleNode
    /// </summary>
    /// <param name="profileTitleNode"></param>
    /// <returns></returns>
    public static string ParseUserName(HtmlNode profileTitleNode) {
        return profileTitleNode
            .DirectDescendants("h1")
            .Select(
                h1 => CleanUsername(h1.InnerText)
            )
            .First();
    }

    /// <summary>
    /// Parses the profile title: which is at path
    /// `h4#js-status-message`
    /// from profileTitleNode
    /// </summary>
    /// <param name="profileTitleNode"></param>
    /// <returns></returns>
    public static string ParseProfileTitle(HtmlNode profileTitleNode) {
        return profileTitleNode
            .FirstDirectDescendant(
                "h4",
                h4 => h4.HasClass("js-status-message")
            )
            .InnerText;
    }
        
    /// <summary>
    /// Parses the avatar url: which is at path
    /// `section.profile-avatar>div.avatar>img`
    /// from profileHeaderNode
    /// </summary>
    /// <param name="profileHeaderNode"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Parses number of forum posts, which is at path
    /// `tr>td[1]`
    /// from profileStatsNode
    /// </summary>
    /// <param name="profileStatsNode"></param>
    /// <returns></returns>
    public static string ParseForumPosts(HtmlNode profileStatsNode) {
        return profileStatsNode
            .FirstDirectDescendant(
                "tr"
            )
            .DirectDescendants("td")
            .Position(1)
            .InnerText;       
    }
    
    /// <summary>
    /// Parses number of forum posts, which is at path
    /// `tr>td[2]`
    /// from profileStatsNode
    /// </summary>
    /// <param name="profileStatsNode"></param>
    /// <returns></returns>
    public static string ParseWikiPoints(HtmlNode profileStatsNode) {
        return profileStatsNode
            .FirstDirectDescendant(
                "tr"
            )
            .DirectDescendants("td")
            .Position(2)
            .InnerText;
    }

    public static LinkPreview ParseFollowing(HtmlNode profileStatsNode) {
        return profileStatsNode
            .FirstDirectDescendant(
                "tr"
            )
            .DirectDescendants("td")
            .Position(3)
            .DirectDescendants("a")
            .Select(
                aNode => new LinkPreview()
                {
                    Url = aNode.GetAttributeValue("href", null) ??
                           throw new UserParserException(nameof(ParseFollowers)),
                    Text = aNode.InnerText
                }
            )
            .First();
    }

    /// <summary>
    /// Parses number of followers, which is at path
    /// `tr>td[4]>a`
    /// from profileStatsNode
    /// </summary>
    /// <param name="profileStatsNode"></param>
    /// <returns></returns>
    /// <exception cref="UserParserException"></exception>
    public static LinkPreview ParseFollowers(HtmlNode profileStatsNode) {
        return profileStatsNode
            .FirstDirectDescendant(
                "tr"
            )
            .DirectDescendants("td")
            .Position(4)
            .DirectDescendants("a")
            .Select(
                aNode => new LinkPreview()
                {
                    Url = aNode.GetAttributeValue("href", null) ??
                           throw new UserParserException(nameof(ParseFollowers)),
                    Text = aNode.InnerText
                }
            )
            .First();
    }

    /// <summary>
    /// Parse cover picture, which is at path
    /// `div.profile-image>div.img>a.imgflare.imgclass>img`
    /// from asideNode
    /// </summary>
    /// <param name="asideNode"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Parses about me section, which is at the asideNode
    /// </summary>
    /// <param name="asideNode"></param>
    /// <returns></returns>
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
    
    // /// <summary>
    // /// </summary>
    // /// <param name="asideNode">the asideNode</param>
    // /// <returns>asidePod</returns>
    // public static HtmlNode? GetLatestImageAsidePod(HtmlNode asideNode) {
    //     return asideNode
    //         .FirstDirectDescendantOrDefault(
    //             "div",
    //             div => 
    //                 div
    //                     .HasClass("aside-pod") && 
    //                div.ChildNodes
    //                    .Any(
    //                        each => each.Name=="div" && 
    //                                 each.HasClass("pod-body") && 
    //                                 each.HasClass("gallery-box-pod")
    //                    )
    //         );
    // }

    public static UserActivity ParseActivity(HtmlNode liActivityNode) {
        HtmlNode mediaBody = liActivityNode
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("media")
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("media-body")
            ); 
        
        HtmlNode activityMessageNode = mediaBody
            .FirstDirectDescendant(
                "span",
                span => span.HasClass("activity-message")
            ); 
        HtmlNode activityContentNode = mediaBody
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("activity-content")
            );       
         
        UserActivity activity = new();

        activity.ActivityTime = activityMessageNode
            .FirstDirectDescendant("time")
            .InnerText;

        string activityType = activityMessageNode
            .FirstDirectDescendant("i")
            .FirstDirectDescendant("svg")
            .GetAttributeValue("class", "")
            .Split(" ")[1];
        
        switch (activityType) {
            case "symbol-picture":
                activity.Type = ActivityType.Image;
                activity.ImageActivity = new ImageActivity()
                {
                    ImageUrl = activityContentNode
                        .FirstDirectDescendant("figure")
                        .FirstDirectDescendant("a")
                        .FirstDirectDescendant("img")
                        .GetAttributeValue("src", "")
                };
                break;
            
            case "symbol-smile":
                activity.Type = ActivityType.Follow;
                activity.FollowActivity = new FollowActivity()
                {
                    User = new LinkPreview()
                    {
                        Url = activityMessageNode
                            .FirstDirectDescendant("a")
                            .GetAttributeValue("href", ""),
                        Text = activityMessageNode
                            .FirstDirectDescendant("a")
                            .InnerText
                            .Trim()
                    }
                };
                break;
            
            case "symbol-comments-alt":
                activity.Type = ActivityType.Comment;
                activity.CommentActivity = new CommentActivity()
                {
                    Content = activityContentNode
                        .FirstDirectDescendant("p")
                        .InnerHtml,
                    
                    Topic = new LinkPreview()
                    {
                        Url = activityMessageNode
                            .DirectDescendants("a")
                            .Skip(0)
                            .First()
                            .GetAttributeValue("href", ""),
                        Text = activityMessageNode
                            .DirectDescendants("a")
                            .Skip(0)
                            .First()
                            .InnerHtml
                    },
                    
                    Forum = new LinkPreview()
                    {
                        Url = activityMessageNode
                            .DirectDescendants("a")
                            .Skip(1)
                            .First()
                            .GetAttributeValue("href", ""),
                        Text = activityMessageNode
                            .DirectDescendants("a")
                            .Skip(1)
                            .First()
                            .InnerHtml
                    }
                };
                break;
            
            default:
                throw new UserParserException(nameof(ParseActivity));
        }
        
        return activity;
    }

    /// <summary>
    /// Parses user activities which is at path
    /// `li.activity-list__item`
    /// from wrapperNode
    /// </summary>
    /// <param name="wrapperNode"></param>
    /// <returns></returns>
    public static UserActivity[]? ParseUserActivities(HtmlNode wrapperNode) {
        HtmlNode activityNode = GetActivityListNode(wrapperNode);

        if (activityNode.FirstDirectDescendant("li").HasClass("none")) return null;
        
        return activityNode
            .DirectDescendants("li")
            .Where(li => li.HasClass("activity-list__item"))
            .Select(ParseActivity)
            .ToArray();
    }

    /// <summary>
    /// Parses background image which is at path
    /// `div#js-kubrick-lead`
    /// from wrapperNode
    /// </summary>
    /// <param name="wrapperNode"></param>
    /// <returns></returns>
    public static string ParseBackgroundImage(HtmlNode wrapperNode) {
        return wrapperNode
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("id", "") == "js-kubrick-lead"
            )
            .GetAttributeValue("style", "")
            .Split("background-image: url(")[1][..^1];
    }
    
    public static Profile Parse<T>(HtmlNode rootNode, ILogger<T> logger) {
        Stopwatch timer = Stopwatch.StartNew();
        
        HtmlNode wrapperNode = MainParser.GetWrapperNode(rootNode);
        
        HtmlNode asideNode = GetAsideNode(wrapperNode);
        
        HtmlNode profileHeaderNode = GetProfileHeaderContainer(wrapperNode);
        HtmlNode profileStatsNode = GetStatsNode(profileHeaderNode); 
        HtmlNode profileTitleNode = GetProfileTitleNode(profileHeaderNode);
        
        Profile profile = new Profile(); 
        profile.Activities   = ParseUserActivities(wrapperNode);
        profile.AvatarUrl    = ParseAvatarUrl(profileHeaderNode);
        profile.BackgroundImage = ParseBackgroundImage(wrapperNode);
        
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