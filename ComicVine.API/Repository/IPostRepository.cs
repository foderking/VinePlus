using ComicVine.API.Controllers;
using ComicVine.API.Models;

namespace ComicVine.API.Repository;

public interface IPostRepository
{
    public Task<PostPage> GetPostPage(string path, int pageNo, ILogger<PostController> logger);
}