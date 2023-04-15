using System.Diagnostics;
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
 
    public Task<IEnumerable<Comicvine.Core.Parsers.Blog>> GetBlog(string username) {
        // Stream stream = await Comicvine.Core.Net.getStream($"/profile/{username}/blog");
        // HtmlNode rootNode = Net.getRootNode(stream);
        return Comicvine.Core.Parsers.ParseBlogsFull($"/profile/{username}/blog");
        // return Comicvine.Core.Parsers.ParseBlogsFull(rootNode);
    }
     public async Task<Comicvine.Core.Parsers.Image> GetImage(string username) {
        Stream stream = await Net.getStream($"/profile/{username}/images");
        
        HtmlNode rootNode = Net.getRootNode(stream);
        Comicvine.Core.Parsers.Image image = Comicvine.Core.Parsers.parseImages(rootNode);

        return image;
    }
   
 
    public async Task<IEnumerable<Comicvine.Core.Parsers.Following>> GetUserFollowing(string username) {
        // Stopwatch timer   = Stopwatch.StartNew();
        
        // await using Stream stream = await Repository.GetStream($"/profile/{username}/following", new ( new []
        Stream stream = await Net.getStream($"/profile/{username}/following");
        HtmlNode rootNode = Net.getRootNode(stream);

        var followings = Comicvine.Core.Parsers.parseFollowings(rootNode);
        
        // timer.Stop();
        // logger.LogInformation("Request to /profile/{Username} completed in {Elapsed}", username, Repository.GetElapsed(timer.Elapsed));
        return followings;
    }

    public async Task<IEnumerable<Comicvine.Core.Parsers.Follower>> GetUserFollowers(string username) {
                
        Stream stream = await Net.getStream($"/profile/{username}/follower");
        HtmlNode rootNode = Net.getRootNode(stream);

        var followers = Comicvine.Core.Parsers.parseFollowers(rootNode);

        return followers;
    }
   
    
    
    
  
    public async Task<Profile> GetProfile(string username, ILogger<ProfileController> logger) {
        
        await using Stream stream     = await Repository.GetStream($"/profile/{username}");
        HtmlNode rootNode = Repository.GetRootNode(stream);
        Profile parsedProfile   = ProfileParser.Parse(rootNode, logger);
        
        return parsedProfile;
    }


}    
