using ComicVine.API.Controllers;
using ComicVine.API.Models;

namespace ComicVine.API.Repository;

public interface IForumRepository
{
    public Task<ForumPage> GetForumPage(int pageNo, ILogger<ForumController> logger);
}