// using ComicVine.API.Services;
// using Comicvine.Core;
// using Microsoft.AspNetCore.Mvc;
//
// namespace ComicVine.API.Controllers;
//
// [ApiController]
// [Route("api/[controller]")]
// public class ProfileController : ControllerBase
// {
//     private readonly IProfileService<ProfileController> _profileService;
//     private readonly ILogger<ProfileController> _logger;
//
//     public ProfileController(IProfileService<ProfileController> profileService, ILogger<ProfileController> logger) {
//         _profileService = profileService;
//         _logger = logger;
//     }
//
//     /// <summary>
//     /// Gets the profile information a comicvine user
//     /// </summary>
//     /// <param name="username">The specified user</param>
//     /// <returns>A single json containing details about the specified user</returns>
//     [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Parsers.Profile))]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [HttpGet("{username}")]
//     public async Task<IActionResult> GetUser(string username) {
//         try {
//             Parsers.Profile profile = await _profileService.GetProfile(username);
//             return Ok(profile);
//         }
//         catch (HttpRequestException) {
//             return NotFound();
//         }
//     }
//
//     /// <summary>
//     /// Gets the people a comicvine user is following
//     /// </summary>
//     /// <param name="username">The specified user</param>
//     /// <returns>An array of json containing details of the people/things the user is following</returns>
//     [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Parsers.Following>))]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [HttpGet("{username}/following")]
//     public async Task<IActionResult> GetFollowing(string username) {
//         try {
//             IEnumerable<Parsers.Following> following = await _profileService.GetUserFollowing(username);
//             return Ok(following);
//         }
//         catch (HttpRequestException) {
//             return NotFound();
//         }
//     }
//
//     /// <summary>
//     /// Gets a comicvine user's followers
//     /// </summary>
//     /// <param name="username">The specified user</param>
//     /// <returns>An array of json representing some info on the user's followers</returns>
//     [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Parsers.Follower>))]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [HttpGet("{username}/followers")]
//     public async Task<ActionResult> GetFollowers(string username) {
//         try {
//             IEnumerable<Parsers.Follower> followers = await _profileService.GetUserFollowers(username);
//             return Ok(followers);
//         }
//         catch (HttpRequestException) {
//             return NotFound();
//         }
//     }
//
//     /// <summary>
//     /// Gets all the blog posts by a comicvine user
//     /// </summary>
//     /// <param name="username">The specified user</param>
//     /// <returns>An array of json containing details for each blog post</returns>
//     [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Parsers.Blog>))]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [HttpGet("{username}/blog")]
//     public async Task<ActionResult> GetBlog(string username) {
//         try {
//             IEnumerable<Parsers.Blog> blogPage = await _profileService.GetBlog(username);
//             return Ok(blogPage);
//         }
//         catch (HttpRequestException) {
//             return NotFound();
//         }
//     }
//
//     /// <summary>
//     /// Gets image details for user
//     /// </summary>
//     /// <param name="username">The specified user</param>
//     /// <returns>A json representing parameters to help query user images</returns>
//     [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Parsers.Image))]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     [HttpGet("{username}/images")]
//     public async Task<ActionResult<Parsers.Image>> GetImages(string username) {
//         try {
//             Parsers.Image imagePage = await _profileService.GetImage(username);
//             return Ok(imagePage);
//         }
//         catch (HttpRequestException) {
//             return NotFound();
//         }
//     }
// }