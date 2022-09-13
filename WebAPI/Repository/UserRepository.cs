using System.Diagnostics;
using HtmlAgilityPack;
using WebAPI.Controllers;
using WebAPI.Models;
using WebAPI.Repository.Parsers;
using Exception = System.Exception;

namespace WebAPI.Repository;

public class UserRepository: IUserRepository<ProfileController>
{
   
    public Task<Stream> GetStream(string username) {
        return Repository.GetStream(username);
    }


    public async Task<Profile> GetProfile(string username, ILogger<ProfileController> logger) {
        Stopwatch timer   = Stopwatch.StartNew();
        Stream stream     = await Repository.GetStream($"/profile/{username}");;
        HtmlNode rootNode = Repository.GetRootNode(stream);
        Profile parsedProfile   = UserParser.Parse(rootNode, logger);
        timer.Stop();
        logger.LogInformation($"Request to /profile/{username} completed in {Repository.GetElapsed(timer.Elapsed)}");
        return parsedProfile;
    }

    public async Task GetUserBlog(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserImages(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserForumPosts(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetWikiPosts(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserFollowing(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserFollowers(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserLists(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserReviews(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }
}    
