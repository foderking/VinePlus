using ComicVine.API.Controllers;
using ComicVine.API.Models;
using ComicVine.API.Repository.Parsers;
using Comicvine.Core;
using HtmlAgilityPack;

namespace ComicVine.API.Repository;

public interface IForumRepository
{
    public Task<IEnumerable<Comicvine.Core.Parsers.Thread>> GetForumPage(int pageNo, ILogger<ForumController> logger);
}

public class ForumRepository : IForumRepository
{
    public async Task<IEnumerable<Comicvine.Core.Parsers.Thread>> GetForumPage(int pageNo, ILogger<ForumController>? logger = null) {
        // await using Stream stream = await Repository.GetStream($"/forums", new ( new []
        // {
        //     new KeyValuePair<string, string>("page", pageNo.ToString())
        // }) );
        Stream stream = await Net.getStreamByPage(pageNo, "forums");
        HtmlNode rootNode = Repository.GetRootNode(stream);
        // ForumPage forumPage = ForumParser.Parse(rootNode, pageNo, logger);
        return Comicvine.Core.Parsers.parseThread(rootNode);
    }

}