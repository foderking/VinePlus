using Comicvine.Core;
using HtmlAgilityPack;

namespace ComicVine.API.Repository;

public interface IForumRepository
{
    public Task<IEnumerable<Comicvine.Core.Parsers.Thread>> GetForumPage(int pageNo);
}

public class ForumRepository : IForumRepository
{
    private static Comicvine.Core.Parsers.ThreadParser _parser = new();
    
    public async Task<IEnumerable<Comicvine.Core.Parsers.Thread>> GetForumPage(int pageNo) {
        Stream stream = await Net.getStreamByPage(pageNo, "forums");
        HtmlNode rootNode = Repository.GetRootNode(stream);
        return _parser.ParseSingle(rootNode);
    }

}