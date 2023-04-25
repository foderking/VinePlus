using ComicVine.API.Repository;
using Comicvine.Core;
using Microsoft.AspNetCore.Mvc;

namespace ComicVine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly ILogger<PostController> _logger;
    private readonly IPostRepository _postRepo;

    public PostController(ILogger<PostController> logger, IPostRepository posRepo) {
        _logger = logger;
        _postRepo = posRepo;
    }
    
    /// <summary>
    /// Gets all of the posts made in a particular thread
    /// </summary>
    /// <param name="path">The path to the particular thread (eg "/forums/hulk-vs-superman/")</param>
    /// <returns>An array of json containing details on each post made in the thread</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Parsers.Post>>> GetPage([FromQuery]string path) {
        try {
            var postPage = await _postRepo.GetPost(path);
            return Ok(postPage);
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }

    /// <summary>
    /// Gets all of the posts made in a particular page of a particular thread
    /// </summary>
    /// <param name="path">The path to the particular thread (eg "/forums/hulk-vs-superman/")</param>
    /// <param name="page">The page number</param>
    /// <returns>An array of json containing details on each post made in that particular page</returns>
    [HttpGet("{page}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Parsers.Post>>> GetPage([FromQuery]string path, int page) {
        try {
            var postPage = await _postRepo.GetSinglePost(path, page);
            return Ok(postPage);
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }
}