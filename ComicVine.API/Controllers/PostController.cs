// using ComicVine.API.Services;
// using Comicvine.Core;
// using HtmlAgilityPack;
// using Microsoft.AspNetCore.Mvc;
//
// namespace ComicVine.API.Controllers;
//
// [ApiController]
// [Route("api/[controller]")]
// public class PostController : ControllerBase
// {
//     private readonly ILogger<PostController> _logger;
//     private readonly Parsers.IMultiple<Parsers.Post> _postRepo;
//
//     public PostController(ILogger<PostController> logger, Parsers.IMultiple<Parsers.Post> posRepo) {
//         _logger = logger;
//         _postRepo = posRepo;
//     }
//     
//     /// <summary>
//     /// Gets all of the posts made in a particular thread
//     /// </summary>
//     /// <param name="path">The path to the particular thread (eg "/forums/hulk-vs-superman/")</param>
//     /// <returns>An array of json containing details on each post made in the thread</returns>
//     [HttpGet]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<ActionResult<IEnumerable<Parsers.Post>>> GetPage([FromQuery]string path) {
//         try {
//             var postPage = await _postRepo.ParseAll(path);
//             return Ok(postPage);
//         }
//         catch (HttpRequestException) {
//             return NotFound();
//         }
//     }
//
//     /// <summary>
//     /// Gets all of the posts made in a particular page of a particular thread
//     /// </summary>
//     /// <param name="path">The path to the particular thread (eg "/forums/hulk-vs-superman/")</param>
//     /// <param name="page">The page number</param>
//     /// <returns>An array of json containing details on each post made in that particular page</returns>
//     [HttpGet("{page}")]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status404NotFound)]
//     public async Task<ActionResult<IEnumerable<Parsers.Post>>> GetPage([FromQuery]string path, int page) {
//         try {
//             Stream stream = await Net.getStreamByPage(page, path);
//             HtmlNode rootNode = Net.getRootNode(stream);
//             var postPage = _postRepo.ParseSingle(rootNode);
//             return Ok(postPage);
//         }
//         catch (HttpRequestException) {
//             return NotFound();
//         }
//     }
// }