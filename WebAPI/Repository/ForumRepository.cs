using HtmlAgilityPack;
using WebAPI.Controllers;
using WebAPI.Models;
using WebAPI.Repository.Parsers;

namespace WebAPI.Repository;

public class ForumRepository : IForumRepository
{
    public async Task<ForumPage> GetForumPage(int pageNo, ILogger<ForumController> logger) {
        await using Stream stream = await Repository.GetStream($"/forums", new ( new []
        {
            new KeyValuePair<string, string>("page", pageNo.ToString())
        }) );
        HtmlNode rootNode = Repository.GetRootNode(stream);

        ForumPage forumPage = ForumParser.Parse(rootNode, pageNo, logger);

        return forumPage;
    }

}