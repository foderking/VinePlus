using ComicVine.API.Models;
using HtmlAgilityPack;

namespace ComicVine.API.Repository.Parsers;
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

        string lastPostPage =  lastPostLink
            .Split("#")[0]
            .Split("=")[1];
        
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
        var NoViews = int.Parse(views);
        var NoPost  = int.Parse(posts);
        var MostRecentPostNo   = int.Parse(lastPostNo);
        var MostRecentPostPage = int.Parse(lastPostPage);
        var DateCreated = DateTime.Parse(dateCreated);
        var CreatorName = creator;
        var CreatorLink = creatorLink;
        var ThreadTitle = type is "Question" or "Answered" ?  threadTitle[3..] : threadTitle;
        var ThreadLink  = threadLink;
        var ThreadType  = type;
        var BoardName   = board;
        var BoardLink   = boardLink;

        int Id = int.Parse(
            flexNode
                .FirstDirectDescendant(
                    "div",
                    div => div.HasClass("js-posts")
                )
                .FirstDirectDescendant(
                    "meta",
                    meta => meta.HasClass("js-post-render-topic")
                )
                .GetAttributeValue("data-post-render-value", "")
        );
        // try {
        //     Id = int.Parse(
        //         threadLink
        //             .Split("-")
        //             .Last()
        //             .Split("/")
        //             .First()
        //     );
        // }
        // catch (Exception) {
        //     Console.WriteLine("asf");
        // }

        var IsLocked = topicNode
            .FirstDirectDescendantOrDefault(
                "img",
                img => img.GetAttributeValue("src", "") ==
                       "https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-lock-16x16.png"
            ) is not null;
        
        var IsPinned = topicNode
            .FirstDirectDescendantOrDefault(
                "img",
                img => img.GetAttributeValue("src", "") ==
                       "https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-pin-16x16.png"
            ) is not null;
        
        return new ForumThread(Id, ThreadTitle, ThreadLink, BoardName, BoardLink, IsLocked, IsPinned, ThreadType,
            MostRecentPostNo, MostRecentPostPage, DateCreated, NoPost, NoViews, creatorLink, CreatorName, new());
    }

    public static ForumThread[] ParseThreads(HtmlNode wrapperNode) {
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

    public static ForumPage Parse<T>(HtmlNode rootNode, int pageNo, ILogger<T>? logger) {
        HtmlNode wrapperNode = MainParser.GetWrapperNode(rootNode);
        ForumPage forumPage = new();
        forumPage.PageNo = pageNo;
        forumPage.ForumThreads = ParseThreads(wrapperNode);
        forumPage.TotalPages = int.Parse(
            wrapperNode
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
                    div => div.HasClass("forum-bar")
                )
                .FirstDirectDescendant(
                    "ul",
                    ul => ul.HasClass("paginate") 
                )
                .DirectDescendants(
                    "li",
                    li => li.GetAttributeValue("class", "") == "paginate__item" ||  li.GetAttributeValue("class", "") == "paginate__item on"
                )
                .Last().InnerText.Trim()
        );
        return forumPage;
    }
}