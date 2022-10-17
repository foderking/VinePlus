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
        catch (HttpRequestException) {
            return NotFound();
        }
    }

    /// <summary>
    /// Gets the people a comicvine user is following
    /// Produces a 404 if the page does not exist
    /// </summary>
    /// <param name="username"></param>
    /// <param name="pageNo">the current page</param>
    /// <returns></returns>
    [HttpGet("{username}/following")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FollowingPage>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowing(string username, [FromQuery(Name = "page")] int pageNo) {
        try {

            FollowingPage following = await _userRepository.GetUserFollowing(username, Math.Max(pageNo, 1), _logger);
            return Ok(following);
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }

    /// <summary>
    /// Gets a comicvine user's followers
    /// </summary>
    /// <param name="username"></param>
    /// <param name="pageNo">the page number</param>
    /// <returns></returns>
    [HttpGet("{username}/followers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FollowersPage>> GetFollowers(string username, [FromQuery(Name = "page")] int pageNo) {
        try {
            FollowersPage followers = await _userRepository.GetUserFollowers(username, Math.Max(pageNo, 1), _logger);
            return Ok(followers);
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }

    /// <summary>
    /// Gets all the blog posts by a comicvine user
    /// </summary>
    /// <param name="username"></param>
    /// <param name="pageNo">the page number</param>
    /// <returns></returns>
    [HttpGet("{username}/blog")]
    public async Task<ActionResult<BlogPage>> GetBlog(string username, [FromQuery(Name = "page")] int pageNo) {
        try {
            BlogPage blogPage = await _userRepository.GetUserBlog(username, Math.Max(pageNo, 1), _logger);
            return Ok(blogPage);
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }

    /// <summary>
    /// Gets all images a comicvine user has posted
    /// </summary>
    /// <param name="username"></param>
    /// <param name="pageNo">page number</param>
    /// <returns></returns>
    [HttpGet("{username}/images")]
    public async Task<ActionResult<ImagePage>> GetImages(string username, [FromQuery(Name = "page")] int pageNo) {
        try {
            ImagePage imagePage = await _userRepository.GetUserImages(username, Math.Max(pageNo, 1), _logger);
            return Ok(imagePage);
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }

    // /// <summary>
    // /// Gets all forums posts by a comicvine user
    // /// </summary>
    // /// <param name="username"></param>
    // /// <returns></returns>
    // [HttpGet("{username}/forums")]
    // public Task GetForum(string username) {
    //     return _userRepository.GetUserForumPosts(username);
    // }

    // /// <summary>
    // /// Gets a comicvine users wiki submissions
    // /// </summary>
    // /// <param name="username"></param>
    // /// <returns></returns>
    // [HttpGet("{username}/wiki-submissions")]
    // public Task GetWiki(string username) {
    //     return _userRepository.GetWikiPosts(username);
    // }
    //
    // /// <summary>
    // /// Gets all lists made by a comicvine user
    // /// </summary>
    // /// <param name="username"></param>
    // /// <returns></returns>
    // [HttpGet("{username}/lists")]
    // public Task GetLists(string username) {
    //     return _userRepository.GetUserLists(username);
    // }
    //
    // /// <summary>
    // /// Gets all reviews made by a comicvine user
    // /// </summary>
    // /// <param name="username"></param>
    // /// <returns></returns>
    // [HttpGet("{username}/reviews")]
    // public Task GetReviews(string username) {
    //     return _userRepository.GetUserReviews(username);
    // }
}