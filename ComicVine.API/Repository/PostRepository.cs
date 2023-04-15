namespace ComicVine.API.Repository;

public interface IPostRepository
{
    public Task<IEnumerable<Comicvine.Core.Parsers.Post>> GetPost(string path);
}

public class PostRepository: IPostRepository
{
    private static Comicvine.Core.Parsers.PostParser _parser = new();
    
    public async Task<IEnumerable<Comicvine.Core.Parsers.Post>> GetPost(string path) {

        var res = await _parser.ParseAll(path);
        return res;
    }
}