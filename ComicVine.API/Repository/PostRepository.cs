using System.Net;
using ComicVine.API.Controllers;
using ComicVine.API.Models;
using ComicVine.API.Repository.Parsers;
using HtmlAgilityPack;

namespace ComicVine.API.Repository;

public interface IPostRepository
{
    public Task<PostPage> GetPostPage(string path, int pageNo, ILogger<PostController> logger);
}

public class PostRepository: IPostRepository
{

    public async Task<PostPage> GetPostPage(string path, int pageNo, ILogger<PostController>? logger = null) {
        // try {
        await using Stream stream = await Repository.GetStream(path, new(new[]
        {
            new KeyValuePair<string, string>("page", pageNo.ToString())
        }));
        int id = int.Parse(path.Split("-").Last().Split('/')[0]);
        HtmlNode rootNode = Repository.GetRootNode(stream);

        PostPage postPage = PostParser.Parse(rootNode, pageNo, id, logger);

        return postPage;

        // }
        // catch (HttpRequestException e) {
        //     if (e.StatusCode == HttpStatusCode.NotFound) {
        //         return null;
        //     }
        //
        //     Console.WriteLine("Error: {0} in {1}", e.Message,path);
        //     return await GetPostPage(path, pageNo, logger);
        // }
    }
}