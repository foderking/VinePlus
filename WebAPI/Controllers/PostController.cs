using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Repository;

namespace WebAPI.Controllers;

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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostPage>> GetPage([FromQuery(Name = "page")] int pageNo, [FromQuery(Name = "threadPath")] string path) {
        try {
            PostPage postPage = await _postRepo.GetPostPage(path, Math.Max(1, pageNo), _logger);
            return postPage;
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }

}