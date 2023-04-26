// using ComicVine.API.Services;
// using Comicvine.Core;
// using HtmlAgilityPack;
// using Microsoft.AspNetCore.Mvc;
//
// namespace ComicVine.API.Controllers;
//
// [ApiController]
// [Route("api/[controller]")]
// public class ForumController : ControllerBase
// {
//     private readonly ILogger<ForumController> _logger;
//     private readonly Parsers.IMultiple<Parsers.Thread> _forumRepo;
//
//     public ForumController(ILogger<ForumController> logger, Parsers.IMultiple<Parsers.Thread> forumRepo) {
//         _logger = logger;
//         _forumRepo = forumRepo;
//     }
//     
//     /// <summary>
//     /// Gets all the forum posts on the first page
//     /// </summary>
//     /// <returns>An array of json containing details for all threads on the first page</returns>
//     [HttpGet]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<ActionResult<IEnumerable<Parsers.Thread>>> Get() {
//         try {
//             Stream stream = await Net.getStreamByPage(1, "forums");
//             HtmlNode rootNode = Net.getRootNode(stream);
//             var ans = _forumRepo.ParseSingle(rootNode);
//             return Ok(ans);
//         }
//         catch (HttpRequestException) {
//             return NotFound();
//         }
//     }
//     
//     /// <summary>
//     /// Gets all the forum posts at a particular page
//     /// </summary>
//     /// <param name="pageNo">The page number</param>
//     /// <returns>An array of json containing details for all threads in the particular page</returns>
//     [HttpGet("{pageNo}")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<ActionResult<IEnumerable<Parsers.Thread>>> GetPage(int pageNo) {
//         try {
//             Stream stream = await Net.getStreamByPage(pageNo, "forums");
//             HtmlNode rootNode = Net.getRootNode(stream);
//             var ans = _forumRepo.ParseSingle(rootNode);
//             return Ok(ans);
//         }
//         catch (HttpRequestException) {
//             return NotFound();
//         }
//     }
// }