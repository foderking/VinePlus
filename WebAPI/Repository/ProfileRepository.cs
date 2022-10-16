using System.Diagnostics;
using HtmlAgilityPack;
using WebAPI.Controllers;
using WebAPI.Models;
using WebAPI.Repository.Parsers;

namespace WebAPI.Repository;

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

    public async Task GetUserImages(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserForumPosts(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

}    
