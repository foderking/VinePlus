using System.Diagnostics;
using ComicVine.API.Controllers;
using ComicVine.API.Models;
using ComicVine.API.Repository.Parsers;
using HtmlAgilityPack;

namespace ComicVine.API.Repository;

public interface IUserRepository<T>
{
    public Task<Profile> GetProfile(string username, ILogger<T> logger);
    public Task<FollowingPage> GetUserFollowing(string username, int pageNo, ILogger<T> logger);
    public Task<FollowersPage> GetUserFollowers(string username, int pageNo, ILogger<T> logger);
    public Task<BlogPage> GetUserBlog(string username, int pageNo, ILogger<T> logger);
    public Task<ImagePage> GetUserImages(string username, int pageNo, ILogger<T> logger);
    public Task GetUserForumPosts(string username);
    public Task<IEnumerable<Comicvine.Core.Parsers.Blog>> GetBlog(string username);
}

public class UserRepository: IUserRepository<ProfileController>
{
   
    public Task<Stream> GetStream(string username) {
        return Repository.GetStream(username);
    }

    public async Task<Profile> GetProfile(string username, ILogger<ProfileController> logger) {
        // Stopwatch timer   = Stopwatch.StartNew();
        
        await using Stream stream     = await Repository.GetStream($"/profile/{username}");
        HtmlNode rootNode = Repository.GetRootNode(stream);
        Profile parsedProfile   = ProfileParser.Parse(rootNode, logger);
        
        // timer.Stop();
        // logger.LogInformation("Request to /profile/{Username} completed in {Elapsed}", username, Repository.GetElapsed(timer.Elapsed));
        return parsedProfile;
    }

    public async Task<FollowingPage> GetUserFollowing(string username, int pageNo, ILogger<ProfileController> logger) {
        // Stopwatch timer   = Stopwatch.StartNew();
        
        await using Stream stream = await Repository.GetStream($"/profile/{username}/following", new ( new []
        {
            new KeyValuePair<string, string>("page", pageNo.ToString())
        }) );
        HtmlNode rootNode = Repository.GetRootNode(stream);

        FollowingPage followingPage = FollowingParser.Parse(rootNode, pageNo, logger);
        
        // timer.Stop();
        // logger.LogInformation("Request to /profile/{Username} completed in {Elapsed}", username, Repository.GetElapsed(timer.Elapsed));
        return followingPage;
    }

    public async Task<FollowersPage> GetUserFollowers(string username, int pageNo, ILogger<ProfileController> logger) {
                
        await using Stream stream = await Repository.GetStream($"/profile/{username}/follower", new ( new []
        {
            new KeyValuePair<string, string>("page", pageNo.ToString())
        }) );
        HtmlNode rootNode = Repository.GetRootNode(stream);

        FollowersPage followersPage = FollowersParser.Parse(rootNode, pageNo, logger);

        return followersPage;
    }

    public async Task<BlogPage> GetUserBlog(string username, int pageNo, ILogger<ProfileController> logger) {
        await using Stream stream = await Repository.GetStream($"/profile/{username}/blog", new ( new []
        {
            new KeyValuePair<string, string>("page", pageNo.ToString())
        }) );
        HtmlNode rootNode = Repository.GetRootNode(stream);

        BlogPage blogPage = BlogParser.Parse(rootNode, pageNo, logger);

        return blogPage;
    }

    public async Task<IEnumerable<Comicvine.Core.Parsers.Blog>> GetBlog(string username) {
        await using Stream stream = await Repository.GetStream($"/profile/{username}/blog");

        HtmlNode rootNode = Repository.GetRootNode(stream);
        return Comicvine.Core.Parsers.ParseBlog(rootNode);
    }

    public async Task<ImagePage> GetUserImages(string username, int pageNo, ILogger<ProfileController> logger) {
        await using Stream stream = await Repository.GetStream($"/profile/{username}/images");
        
        HtmlNode rootNode = Repository.GetRootNode(stream);
        ImagePage imagePage = await ImageParser.Parse(rootNode, pageNo-1, logger);

        return imagePage;
    }

    public async Task GetUserForumPosts(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

}    
