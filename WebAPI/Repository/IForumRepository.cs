using WebAPI.Controllers;
using WebAPI.Models;

namespace WebAPI.Repository;

public interface IForumRepository
{
    public Task<ForumPage> GetForumPage(int pageNo, ILogger<ForumController> logger);
}