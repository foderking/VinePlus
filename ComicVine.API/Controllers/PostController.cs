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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Parsers.Post>> GetPage([FromQuery(Name = "threadPath")] string path) {
        try {
            var postPage = await _postRepo.GetPost(path);
            return Ok(postPage);
        }
        catch (HttpRequestException) {
            return NotFound();
        }
    }

}