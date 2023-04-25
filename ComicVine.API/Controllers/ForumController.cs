using ComicVine.API.Repository;
using Comicvine.Core;
using Microsoft.AspNetCore.Mvc;

namespace ComicVine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForumController : ControllerBase
{
    private readonly ILogger<ForumController> _logger;
    private readonly IForumRepository _forumRepo;

    public ForumController(ILogger<ForumController> logger, IForumRepository forumRepo) {
        _logger = logger;
        _forumRepo = forumRepo;
    }
    
    /// <summary>
    /// Gets all the forum posts on the first page
    /// </summary>
    /// <returns>An array of json containing details for all threads on the first page</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Parsers.Thread>>> Get() {
        try {
            var ans = await _forumRepo.GetForumPage(1);
            return Ok(ans);
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }
    
    /// <summary>
    /// Gets all the forum posts at a particular page
    /// </summary>
    /// <param name="pageNo">The page number</param>
    /// <returns>An array of json containing details for all threads in the particular page</returns>
    [HttpGet("{pageNo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Parsers.Thread>>> GetPage(int pageNo) {
        try {
            var ans = await _forumRepo.GetForumPage(pageNo);
            return Ok(ans);
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }
}