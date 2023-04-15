using Microsoft.AspNetCore.Mvc;

namespace ComicVine.API.Controllers;

[ApiController]
[Route("/")]
public class IndexController : ControllerBase
{
    private ILogger<IndexController> _logger;
    
    public IndexController(ILogger<IndexController> logger) {
        _logger = logger;
    }
    
    /// <summary>
    /// Redirects to /api
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Index() {
        return Redirect("/api");
    }

    [Route("/api/error")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult HandleError() {
        return Problem();
    }
}
