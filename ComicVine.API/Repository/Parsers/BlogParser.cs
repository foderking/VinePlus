using ComicVine.API.Models;
using HtmlAgilityPack;

namespace ComicVine.API.Repository.Parsers;

public class BlogParser
{
    
    public static HtmlNode GetMainNode(HtmlNode wrapperNode) {
        return wrapperNode
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("id", "") == "site"
            )
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("id", "") == "default-content"
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("primary-content")
            );
    }

    public static Blog ParseBlog(HtmlNode articleNode) {
        HtmlNode sectionNode = articleNode
            .FirstDirectDescendant(
                "section",
                section => section.HasClass("news-hdr")
            );
        HtmlNode aNode = sectionNode
            .FirstDirectDescendant("h1")
            .FirstDirectDescendant("a");
        HtmlNode hNode = sectionNode
            .FirstDirectDescendant("h4");
        
        Blog blog = new();

        blog.Link = aNode.GetAttributeValue("href", null);
        blog.Title = aNode.InnerText.Trim();
        blog.DateCreated = DateTime.Parse(
            hNode
                .FirstDirectDescendant("time")
                .GetAttributeValue("datetime", "")
        );
        blog.Comments = int.Parse(
            hNode
                .FirstDirectDescendant("a")
                .InnerText
                .Trim()
                .Split()[0]
        );
        return blog;
        
    }

    public static Blog[] ParseBlogs(HtmlNode mainNode) {
        return mainNode
            .DirectDescendants(
                "article",
                article => article.HasClass("content-body") || article.HasClass("profile-blog") ||
                           article.HasClass("js-profile-blog")
            )
            .Select(ParseBlog)
            .ToArray();
    }
        
    public static BlogPage Parse<T>(HtmlNode rootNode, int pageNo, ILogger<T> logger) {
        HtmlNode wrapperNode = MainParser.GetWrapperNode(rootNode);
        HtmlNode mainNode = GetMainNode(wrapperNode);

        HtmlNode? navNode = mainNode
            .FirstDirectDescendantOrDefault(
                "ul",
                ul => ul.HasClass("paginate")
            );
        
        
        BlogPage blogPage = new();
        blogPage.PageNo = pageNo;
        blogPage.Blogs = ParseBlogs(mainNode);

        if (navNode == null)
            blogPage.TotalPages = 1;
        else {
            blogPage.TotalPages = Int32.Parse(
                navNode
                    .DirectDescendants(
                        "li", 
                        li => !li.HasClass("skip")
                    )
                    .Last()
                    .InnerText
            );
        }
        
        return blogPage;
    }
    
}