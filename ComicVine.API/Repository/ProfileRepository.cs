using ComicVine.API.Controllers;
using Comicvine.Core;
using HtmlAgilityPack;

namespace ComicVine.API.Repository;

public interface IUserRepository<T>
{
    public Task<Comicvine.Core.Parsers.Image> GetImage(string username);
    public Task<IEnumerable<Comicvine.Core.Parsers.Blog>> GetBlog(string username);
    public Task<IEnumerable<Comicvine.Core.Parsers.Following>> GetUserFollowing(string username);
    public Task<IEnumerable<Comicvine.Core.Parsers.Follower>> GetUserFollowers(string username);
    
    
    public Task<Comicvine.Core.Parsers.Profile> GetProfile(string username);
}

public class UserRepository: IUserRepository<ProfileController>
{
    private static Comicvine.Core.Parsers.ImageParser _imageParser = new();
    private static Comicvine.Core.Parsers.BlogParser _blogParser = new();
    private static Comicvine.Core.Parsers.FollowerParser _followerParser = new();
    private static Comicvine.Core.Parsers.FollowingParser _followingParser = new();
    private static Comicvine.Core.Parsers.ProfileParser _profileParser = new();
 
    public Task<IEnumerable<Comicvine.Core.Parsers.Blog>> GetBlog(string username) {
        return _blogParser.ParseAll($"/profile/{username}/blog");
    }
     public async Task<Comicvine.Core.Parsers.Image> GetImage(string username) {
        Stream stream = await Net.getStream($"/profile/{username}/images");
        HtmlNode rootNode = Net.getRootNode(stream);
        return _imageParser.ParseSingle(rootNode);
     }
   
    public Task<IEnumerable<Comicvine.Core.Parsers.Following>> GetUserFollowing(string username) {
        return _followingParser.ParseAll($"/profile/{username}/following");
    }

    public Task<IEnumerable<Comicvine.Core.Parsers.Follower>> GetUserFollowers(string username) {
        return _followerParser.ParseAll($"/profile/{username}/follower");
    }
   
    public async Task<Comicvine.Core.Parsers.Profile> GetProfile(string username) {
        Stream stream = await Net.getStream($"/profile/{username}/images");
        HtmlNode rootNode = Net.getRootNode(stream);
        return _profileParser.ParseSingle(rootNode);
    }
}    
