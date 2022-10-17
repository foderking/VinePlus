using HtmlAgilityPack;
using WebAPI.Models;

namespace WebAPI.Repository.Parsers;

public class ImageParser
{
    public static int ImagePerPage = 30;
    public static string ParseTotalImages(HtmlNode wrapperNode) {
        return wrapperNode
            .FirstDirectDescendant(
                "nav",
                nav => nav.HasClass("sub-nav")
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("container")
            )
            .FirstDirectDescendant("ul")
            .FirstDirectDescendant(
                "li",
                li => li.InnerText.Trim().StartsWith("Images")
            )
            .InnerText.Trim();
    }

    public static HtmlNode GetMainNode(HtmlNode wrapperNode) {
        return wrapperNode
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("id", "") == "site"
            )
            .FirstDirectDescendant(
                "div",
                div => div.GetAttributeValue("id", "") == "gallery-content"
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("primary-content")
            );
    }

    public static HtmlNode GetDataNode(HtmlNode mainNode) {
        return mainNode
            .FirstDirectDescendant(
                "header",
                header => header.HasClass("gallery-header")
            )
            .FirstDirectDescendant(
                "div",
                div => div.HasClass("gallery-header__objects") || div.HasClass("isotope-image")
            )
            .FirstDirectDescendant(
                "ul",
                ul => ul.HasClass("gallery-tags")
            )
            .FirstDirectDescendant(
                "li",
                li => li.HasClass("gallery-tags__item")
            )
            .FirstDirectDescendant(
                "a",
                a => a.GetAttributeValue("id", "") == "galleryMarker"
            );
    }

    public static async Task<ImagePage> Parse<T>(HtmlNode rootNode, int pageNo, ILogger<T> logger) {
        HtmlNode wrapperNode = ProfileParser.GetWrapperNode(rootNode);
        HtmlNode mainNode = GetMainNode(wrapperNode);
        HtmlNode dataNode = GetDataNode(mainNode);
        string totalImages = ParseTotalImages(wrapperNode);
        
        ImagePage imagePage = new();

        imagePage.PageNo = pageNo;
        imagePage.TotalPages = (int) Math.Ceiling(
            float.Parse(
                totalImages
                    .Split("(")[1][..^1]
            ) / ImagePerPage 
        );
        // imagePage.Images = 
        ImageResult? imageRes = await Repository.GetJson<ImageResult>($"/js/image-data.json", new ()
        {
            ["start"]  = (pageNo * ImagePerPage).ToString(),
            ["count"]  = ImagePerPage.ToString(),
            ["object"] = dataNode.GetAttributeValue("data-object-id", ""),
            ["images"] = dataNode.GetAttributeValue("data-gallery-id", "")
        });

        imagePage.Images = imageRes?.images;

        return imagePage;
    }
}