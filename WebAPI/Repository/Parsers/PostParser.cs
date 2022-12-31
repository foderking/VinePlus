using HtmlAgilityPack;
using WebAPI.Models;

namespace WebAPI.Repository.Parsers;

public class PostParser
{
    public static ForumPost ParsePost(HtmlNode postNode) {
        HtmlNode messageNode = postNode
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("message-wrap")
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("message-inner")
            );

        HtmlNode titleNode = messageNode
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("message-title")
            );

        HtmlNode articleNode = messageNode
            .FirstDirectDescendant(
                "article",
                article => article.HasClass("message-content")
            );
        
        ForumPost forumPost = new();
        forumPost.Id = titleNode
            .DirectDescendants(
                "a",
                a => a.GetAttributeValue("name", null) != null
            )
            .Select(each => int.Parse(
                each
                .GetAttributeValue("name", null)
                .Split("-")[^1])
            )
            .First();
        
        forumPost.PostNo = titleNode
             .DirectDescendants(
                "a",
                a => a.GetAttributeValue("name", null) != null
            )
            .Select(each => int.Parse(each.InnerText.Trim()[1..])
            )
            .First();

        forumPost.IsEdited = titleNode.InnerText.Contains("Edited By");
        
        forumPost.CreatorLink = titleNode
            .DirectDescendants(
                "a",
                a => a.HasClass("message-user")
            )
            .Select(each => each.GetAttributeValue("data-user-profile", ""))
            .First();
        forumPost.CreatorName = titleNode
            .DirectDescendants(
                "a",
                a => a.HasClass("message-user")
            )
            .Select(each => each.InnerText.Trim())
            .First();

        forumPost.DateCreated = DateTime.Parse(
            messageNode
                .FirstDirectDescendant(
                    "div",
                    div => div.HasClass("message-options")
                )
                .FirstDirectDescendant(
                    "time",
                    time => time.HasClass("date")
                )
                .GetAttributeValue("datetime", "")
        );

        forumPost.FirstMention = articleNode
            .Descendants("a")
            .Where(
                a => a.GetAttributeValue("data-embed-type", "") == "mention"
            )
            .Select(
                each => each.GetAttributeValue("data-user-slug", null) 
            )
            .FirstOrDefault();

        forumPost.Content = articleNode.InnerHtml;

        return forumPost;
    }
    public static ForumPost[]? ParseThreads(HtmlNode forumBlockNode) {
        return forumBlockNode
            .FirstDirectDescendant(
                "section",
                div => div.HasClass("forum-messages")
            )
            .DirectDescendants(
                "div",
                div => div.HasClass("js-message") && div.GetAttributeValue("id", "").Split("-").Length == 3
            )
            .Select(ParsePost)
            .ToArray();
    }

    public static HtmlNode GetForumBlockNode(HtmlNode wrapperNode) {
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
                div => div.HasClass("js-forum-block")
            );
    }
    
    public static PostPage Parse<T>(HtmlNode rootNode, int pageNo, ILogger<T> logger) {
        HtmlNode wrapperNode = MainParser.GetWrapperNode(rootNode);
        HtmlNode forumBlockNode = GetForumBlockNode(wrapperNode);

        HtmlNode? paginateNode = forumBlockNode
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("forum-bar")
            )
            .FirstDirectDescendantOrDefault(
                "ul",
                ul => ul.HasClass("paginate")
            );
        
        PostPage postPage = new();
        postPage.PageNo = pageNo;
        postPage.ForumThreads = ParseThreads(forumBlockNode);
        
        postPage.TotalPages = paginateNode==null ? 1 : int.Parse(
            paginateNode
                .DirectDescendants(
                    "li",
                    li => li.GetAttributeValue("class", "") == "paginate__item"
                )
                .Select(each => each.InnerText.Trim())
                .Last()
        );
        
        return postPage;
    }}