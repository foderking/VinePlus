using ComicVine.API.Controllers;
using ComicVine.API.Models;
using ComicVine.API.Repository.Parsers;
using Comicvine.Core;
using HtmlAgilityPack;

namespace ComicVine.API.Repository;

public interface IUserRepository<T>
{
    public Task<Comicvine.Core.Parsers.Image> GetImage(string username);
    public Task<IEnumerable<Comicvine.Core.Parsers.Blog>> GetBlog(string username);
    public Task<IEnumerable<Comicvine.Core.Parsers.Following>> GetUserFollowing(string username);
    public Task<IEnumerable<Comicvine.Core.Parsers.Follower>> GetUserFollowers(string username);
    
    
    public Task<Profile> GetProfile(string username, ILogger<T> logger);
}

public class UserRepository: IUserRepository<ProfileController>
{
    private static Comicvine.Core.Parsers.ImageParser _imageParser = new();
    private static Comicvine.Core.Parsers.BlogParser _blogParser = new();
    private static Comicvine.Core.Parsers.FollowerParser _followerParser = new();
    private static Comicvine.Core.Parsers.FollowingParser _followingParser = new();
 
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
   
    
    
    
  
    public async Task<Profile> GetProfile(string username, ILogger<ProfileController> logger) {
        
        await using Stream stream     = await Repository.GetStream($"/profile/{username}");
        HtmlNode rootNode = Repository.GetRootNode(stream);
        Profile parsedProfile   = ProfileParser.Parse(rootNode, logger);
        
        return parsedProfile;
    }
}    
