using HtmlAgilityPack;
using WebAPI.Controllers;
using WebAPI.Models;
using WebAPI.Repository.Parsers;

namespace WebAPI.Repository;

public class PostRepository: IPostRepository
{

    public async Task<PostPage> GetPostPage(string path, int pageNo, ILogger<PostController> logger) {
        await using Stream stream = await Repository.GetStream($"/forums{path}", new(new[]
        {
            new KeyValuePair<string, string>("page", pageNo.ToString())
        }));
        HtmlNode rootNode = Repository.GetRootNode(stream);

        PostPage postPage = PostParser.Parse(rootNode, pageNo, logger);

        return postPage;
    }
}