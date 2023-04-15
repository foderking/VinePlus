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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Parsers.Thread>> GetPage([FromQuery(Name = "page")] int pageNo) {
        try {
            var ans = await _forumRepo.GetForumPage(Math.Max(1, pageNo));
            return Ok(ans);
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }
}