using Comicvine.Core;
using HtmlAgilityPack;

namespace ComicVine.API.Repository;

public interface IPostRepository
{
    public Task<IEnumerable<Comicvine.Core.Parsers.Post>> GetPost(string path);
    public Task<IEnumerable<Comicvine.Core.Parsers.Post>> GetSinglePost(string path, int page);
}

public class PostRepository: IPostRepository
{
    private static Comicvine.Core.Parsers.PostParser _parser = new();
    
    public async Task<IEnumerable<Comicvine.Core.Parsers.Post>> GetPost(string path) {

        var res = await _parser.ParseAll(path);
        return res;
    }
    public async Task<IEnumerable<Comicvine.Core.Parsers.Post>> GetSinglePost(string path, int page) {

        Stream stream = await Net.getStreamByPage(page, path);
        HtmlNode rootNode = Net.getRootNode(stream);
        return _parser.ParseSingle(rootNode);
    }
}