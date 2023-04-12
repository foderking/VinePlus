using ComicVine.API.Models;
using HtmlAgilityPack;

namespace ComicVine.API.Repository.Parsers;

public class PostParser
{
    public static ForumPost ParsePost(HtmlNode postNode, int ThreadId) {
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
        
        var Id = titleNode
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
        
        var PostNo = titleNode
             .DirectDescendants(
                "a",
                a => a.GetAttributeValue("name", null) != null
            )
            .Select(each => int.Parse(each.InnerText.Trim()[1..])
            )
            .First();

        var IsEdited = titleNode.InnerText.Contains("Edited By");
        
        var CreatorLink = titleNode
            .DirectDescendants(
                "a",
                a => a.HasClass("message-user")
            )
            .Select(each => each.GetAttributeValue("data-user-profile", ""))
            .First();
        
        var CreatorName = titleNode
            .DirectDescendants(
                "a",
                a => a.HasClass("message-user")
            )
            .Select(each => each.InnerText.Trim())
            .First();

        var DateCreated = DateTime.Parse(
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

        var FirstMention = articleNode
            .Descendants("a")
            .Where(
                a => a.GetAttributeValue("data-embed-type", "") == "mention"
            )
            .Select(
                each => each.GetAttributeValue("data-user-slug", null) 
            )
            .FirstOrDefault();

        var Content = articleNode.InnerHtml;

        return new (Id, PostNo, FirstMention, CreatorName, CreatorLink, IsEdited, DateCreated, Content, ThreadId);
    }
    public static ForumPost[] ParseThreads(HtmlNode forumBlockNode, int id) {
        bool IsNotEmpty = forumBlockNode
            .FirstDirectDescendant(
                "section",
                div => div.HasClass("forum-messages")
            )
            .DirectDescendants(
                "div",
                div => div.HasClass("js-message") && div.GetAttributeValue("id", "").Split("-").Length == 3
            )
            .Any();
        
        return !IsNotEmpty ? Array.Empty<ForumPost>() : forumBlockNode
            .FirstDirectDescendant(
                "section",
                div => div.HasClass("forum-messages")
            )
            .DirectDescendants(
                "div",
                div => div.HasClass("js-message") && div.GetAttributeValue("id", "").Split("-").Length == 3
            )
            .Select(each => ParsePost(each, id))
            .Select(each =>
            {
                each.ThreadId = id;
                return each;
            })
            .ToArray();
    }

    public static HtmlNode GetForumBlockNode(HtmlNode wrapperNode) {
        if (wrapperNode.FirstDirectDescendantOrDefault("div", div => div.GetAttributeValue("id", "") == "forum-content") != null)
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
        else if (wrapperNode.FirstDirectDescendantOrDefault("div", div => div.HasClass("js-toc-generate")) != null)
            return wrapperNode
                .FirstDirectDescendant(
                    "div",
                    div => div.HasClass("js-toc-generate")
                )
                .FirstDirectDescendant(
                    "div",
                    div => div.GetAttributeValue("id", "") == "site"
                )
                .FirstDirectDescendant(
                    "div",
                    div => div.GetAttributeValue("id", "") == "forum-content"
                )
                .FirstDirectDescendant(
                    "div",
                    div => div.HasClass("primary-content")
                )
                .FirstDirectDescendant(
                    "div",
                    div => div.HasClass("js-forum-block")
                );
        
        else throw new Exception("Unknown Thread Type");
    }
    
    public static PostPage Parse<T>(HtmlNode rootNode, int pageNo, int id, ILogger<T>? logger) {
        HtmlNode wrapperNode = MainParser.GetWrapperNode(rootNode);
        HtmlNode forumBlockNode = GetForumBlockNode(wrapperNode);
        // try {
        HtmlNode? paginateNode = forumBlockNode
            .FirstDirectDescendantOrDefault(
                "div",
                div => div.HasClass("forum-bar")
            )!
            .FirstDirectDescendantOrDefault(
                "ul",
                ul => ul.HasClass("paginate")
            );
        // }
        // catch (Exception) {
        //     paginateNode = null;
        // }

        PostPage postPage = new();
        postPage.PageNo = pageNo;
        postPage.ForumPosts = ParseThreads(forumBlockNode, id);
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