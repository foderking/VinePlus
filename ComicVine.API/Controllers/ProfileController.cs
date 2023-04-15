using ComicVine.API.Repository;
using Comicvine.Core;
using Microsoft.AspNetCore.Mvc;

namespace ComicVine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Parsers.Profile))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(string username) {
        try {
            Parsers.Profile profile = await _userRepository.GetProfile(username);
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
    /// <returns></returns>
    [HttpGet("{username}/following")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Parsers.Following>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowing(string username) {
        try {
            IEnumerable<Parsers.Following> following = await _userRepository.GetUserFollowing(username);
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
    /// <returns></returns>
    [HttpGet("{username}/followers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetFollowers(string username) {
        try {
            IEnumerable<Parsers.Follower> followers = await _userRepository.GetUserFollowers(username);
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
    /// <returns></returns>
    [HttpGet("{username}/blog")]
    public async Task<ActionResult> GetBlog(string username) {
        try {
            IEnumerable<Parsers.Blog> blogPage = await _userRepository.GetBlog(username);
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
    /// <returns></returns>
    [HttpGet("{username}/images")]
    public async Task<ActionResult<Parsers.Image>> GetImages(string username) {
        try {
            Parsers.Image imagePage = await _userRepository.GetImage(username);
            return Ok(imagePage);
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }
}