using WebAPI.Models;

namespace WebAPI.Repository;

public interface IUserRepository
{
    public Task<User> GetProfile(string username);
}