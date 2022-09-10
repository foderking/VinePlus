using WebAPI.Controllers;
using WebAPI.Models;

namespace WebAPI.Repository;

public interface IUserRepository<T>
{
    public Task<User> GetProfile(string username, ILogger<T> logger);
    public Task GetUserBlog(string username);
    public Task GetUserImages(string username);
    public Task GetUserForumPosts(string username);
    public Task GetWikiPosts(string username);
    public Task GetUserFollowing(string username);
    public Task GetUserFollowers(string username);
    public Task GetUserLists(string username);
    public Task GetUserReviews(string username);
}