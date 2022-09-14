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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Profile))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(string username) {
        try {
            Profile profile = await _userRepository.GetProfile(username, _logger);
            return Ok(profile);
        }
        catch (HttpRequestException e) {
            return NotFound();
        }
    }

    /// <summary>
    /// Gets the people a comicvine user is following
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("{username}/following")]
    public Task GetFollowing(string username) {
        return _userRepository.GetUserFollowing(username);
    }

    /// <summary>
    /// Gets a comicvine user's followers
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("{username}/followers")]
    public Task GetFollowers(string username) {
        return _userRepository.GetUserFollowers(username);
    }

    /// <summary>
    /// Gets all the blog posts by a comicvine user
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("{username}/blog")]
    public Task GetBlog(string username) {
        return _userRepository.GetUserBlog(username);
    }

    /// <summary>
    /// Gets all images a comicvine user has posted
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("{username}/images")]
    public Task GetImages(string username) {
        return _userRepository.GetUserImages(username);
    }

    /// <summary>
    /// Gets all forums posts by a comicvine user
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("{username}/forums")]
    public Task GetForum(string username) {
        return _userRepository.GetUserForumPosts(username);
    }

    /// <summary>
    /// Gets a comicvine users wiki submissions
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("{username}/wiki-submissions")]
    public Task GetWiki(string username) {
        return _userRepository.GetWikiPosts(username);
    }

    /// <summary>
    /// Gets all lists made by a comicvine user
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("{username}/lists")]
    public Task GetLists(string username) {
        return _userRepository.GetUserLists(username);
    }

    /// <summary>
    /// Gets all reviews made by a comicvine user
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("{username}/reviews")]
    public Task GetReviews(string username) {
        return _userRepository.GetUserReviews(username);
    }
}