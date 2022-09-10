using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Repository;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController: ControllerBase
{
    private readonly IUserRepository<ProfileController> _userRepository;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IUserRepository<ProfileController> userRepository, ILogger<ProfileController> logger) {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets the default profile view for a comicvine user
    /// </summary>
    /// <param name="username">The username</param>
    /// <returns></returns>
    [HttpGet("{username}")]
    public Task<User> GetUser(string username) {
        return _userRepository.GetProfile(username, _logger);
    }

    [HttpGet("{username}/blog")]
    public Task GetBlog(string username) {
        return _userRepository.GetUserBlog(username);
    }

    [HttpGet("{username}/images")]
    public Task GetImages(string username) {
        return _userRepository.GetUserImages(username);
    }

    [HttpGet("{username}/forums")]
    public Task GetForum(string username) {
        return _userRepository.GetUserForumPosts(username);
    }

    [HttpGet("{username}/wiki-submissions")]
    public Task GetWiki(string username) {
        return _userRepository.GetWikiPosts(username);
    }

    [HttpGet("{username}/following")]
    public Task GetFollowing(string username) {
        return _userRepository.GetUserFollowing(username);
    }

    [HttpGet("{username}/follower")]
    public Task GetFollowers(string username) {
        return _userRepository.GetUserFollowers(username);
    }

    [HttpGet("{username}/lists")]
    public Task GetLists(string username) {
        return _userRepository.GetUserLists(username);
    }

    [HttpGet("{username}/reviews")]
    public Task GetReviews(string username) {
        return _userRepository.GetUserReviews(username);
    }
}