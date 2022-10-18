using HtmlAgilityPack;
using WebAPI.Models;

namespace WebAPI.Repository.Parsers;
public class ForumParserException : Exception
{
    public ForumParserException(string message) : base($"Parsing Error in {message}") {
    }
}
public class ForumParser
{
    public static ForumThread ParseThread(HtmlNode flexNode) {
        string views = flexNode
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("class", "") == "inner-space-small views hide-mobile"
            )
            .InnerText
            .Trim()
            .Replace(",", "");
        
        string posts = flexNode
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("class", "") == "js-posts inner-space-small views"
            )
            .InnerText
            .Trim()
            .Replace(",", "");
        
        string lastPostLink = flexNode
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("class", "") == "inner-space-small last-post hide-mobile"
            )
            .FirstDirectDescendant(
                "span",
                span => span.HasClass("info")
            )
            .FirstDirectDescendant(
                "a",
                a => a.HasClass("last")
            )
            .GetAttributeValue("href", "");
        
        string lastPostNo =  lastPostLink
            .Split("#")[1]
            .Split("-")[^1];

        HtmlNode authorNode = flexNode
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("class", "") == "inner-space-small author hide-laptop"
            ); 

        string dateCreated = authorNode
            .FirstDirectDescendant(
                "span",
                span => span.HasClass("info")
            )
            .InnerText
            .Trim();
        
        string creator = authorNode
            .FirstDirectDescendant(
                "div"
            )
            .FirstDirectDescendant(
                "a"
            )
            .InnerText
            .Trim();

        string creatorLink = authorNode
            .FirstDirectDescendant(
                "div"
            )
            .FirstDirectDescendant(
                "a"
            )
            .GetAttributeValue("href", "");

         string board = flexNode
             .FirstDirectDescendant(
                 "div",
                 div => div.GetAttributeValue("class", "") == "inner-space-small forum-topic"
             )
             .FirstDirectDescendant(
                 "a",
                 a => a.HasClass("board")
             )
             .InnerText
             .Trim();

         string boardLink = flexNode
             .FirstDirectDescendant(
                 "div",
                 div => div.GetAttributeValue("class", "") == "inner-space-small forum-topic"
             )
             .FirstDirectDescendant(
                 "a",
                 a => a.HasClass("board")
             )
             .GetAttributeValue("href", "");
         
         HtmlNode topicNode = flexNode
             .FirstDirectDescendant(
                 "div",
                 div => div.GetAttributeValue("class", "") == "inner-space-small forum-topic"
             )
             .FirstDirectDescendant("div");

         string threadTitle = topicNode
             .FirstDirectDescendant(
                 "a",
                 a => a.HasClass("js-topic-name") && a.HasClass("topic-name")
             )
             .InnerText
             .Trim();
         
          string threadLink = topicNode
             .FirstDirectDescendant(
                 "a",
                 a => a.HasClass("js-topic-name") && a.HasClass("topic-name")
             )
             .GetAttributeValue("href", "");

          string? type = topicNode
              .FirstDirectDescendantOrDefault(
                  "span",
                  span => span.HasClass("type")
              )?.InnerText
              .Trim() ?? "Normal";

          // ThreadType tType = type switch
          // {
          //     null => ThreadType.Normal,
          //     "Poll" => ThreadType.Poll,
          //     "Question" => ThreadType.Question,
          //     "Answered" => ThreadType.Question,
          //     "Blog" => ThreadType.Blog,
          //     _ => throw new ForumParserException("Thread type")
          // };
          
              
        ForumThread forumThread = new();
        forumThread.NoViews = int.Parse(views);
        forumThread.NoPost  = int.Parse(posts);
        forumThread.MostRecentPostNo = int.Parse(lastPostNo);
        forumThread.MostRecentPostLink = lastPostLink;
        forumThread.DateCreated = DateTime.Parse(dateCreated);
        forumThread.CreatorName = creator;
        forumThread.CreatorLink = creatorLink;
        forumThread.ThreadTitle = type is "Question" or "Answered" ?  threadTitle[3..] : threadTitle;
        forumThread.ThreadLink = threadLink;
        forumThread.ThreadType = type;
        forumThread.BoardName = board;
        forumThread.BoardLink = boardLink;

        forumThread.IsLocked = topicNode
            .FirstDirectDescendantOrDefault(
                "img",
                img => img.GetAttributeValue("src", "") ==
                       "https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-lock-16x16.png"
            ) is not null;
        
        forumThread.IsPinned = topicNode
            .FirstDirectDescendantOrDefault(
                "img",
                img => img.GetAttributeValue("src", "") ==
                       "https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-pin-16x16.png"
            ) is not null;
        
        return forumThread;
    }

    public static ForumThread[]? ParseThreads(HtmlNode wrapperNode) {
        return wrapperNode
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("id", "") == "forum-content"
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("three-column--span-two")
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("table-striped") && div.HasClass("table")
            )
            .DirectDescendants(
                "div",
                div => div.HasClass("flexbox-align-stretch")
            )
            .Select(ParseThread)
            .ToArray();
    }

    public static ForumPage Parse<T>(HtmlNode rootNode, int pageNo, ILogger<T> logger) {
        HtmlNode wrapperNode = MainParser.GetWrapperNode(rootNode);
        ForumPage forumPage = new();
        forumPage.PageNo = pageNo;
        forumPage.ForumThreads = ParseThreads(wrapperNode);
        return forumPage;
    }
}