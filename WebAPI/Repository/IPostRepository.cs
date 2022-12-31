using WebAPI.Controllers;
using WebAPI.Models;

namespace WebAPI.Repository;

public interface IPostRepository
{
    public Task<PostPage> GetPostPage(string path, int pageNo, ILogger<PostController> logger);
}