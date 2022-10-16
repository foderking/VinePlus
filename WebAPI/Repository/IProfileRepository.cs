using WebAPI.Controllers;
using WebAPI.Models;

namespace WebAPI.Repository;

public interface IUserRepository<T>
{
    public Task<Profile> GetProfile(string username, ILogger<T> logger);
    public Task<FollowingPage> GetUserFollowing(string username, int pageNo, ILogger<T> logger);
    public Task<FollowersPage> GetUserFollowers(string username, int pageNo, ILogger<T> logger);
    public Task<BlogPage> GetUserBlog(string username, int pageNo, ILogger<T> logger);
    public Task GetUserImages(string username);
    public Task GetUserForumPosts(string username);
}