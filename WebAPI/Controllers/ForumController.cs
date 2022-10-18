using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Repository;

namespace WebAPI.Controllers;

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
    public async Task<ActionResult<ForumPage>> GetPage([FromQuery(Name = "page")] int pageNo) {
        try {
            ForumPage forumPage = await _forumRepo.GetForumPage(Math.Max(1, pageNo), _logger);
            return forumPage;
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }
}