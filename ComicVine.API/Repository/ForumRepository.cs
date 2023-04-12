using ComicVine.API.Controllers;
using ComicVine.API.Models;
using ComicVine.API.Repository.Parsers;
using HtmlAgilityPack;

namespace ComicVine.API.Repository;

public class ForumRepository : IForumRepository
{
    public async Task<ForumPage> GetForumPage(int pageNo, ILogger<ForumController>? logger = null) {
        await using Stream stream = await Repository.GetStream($"/forums", new ( new []
        {
            new KeyValuePair<string, string>("page", pageNo.ToString())
        }) );
        HtmlNode rootNode = Repository.GetRootNode(stream);

        ForumPage forumPage = ForumParser.Parse(rootNode, pageNo, logger);

        return forumPage;
    }

}