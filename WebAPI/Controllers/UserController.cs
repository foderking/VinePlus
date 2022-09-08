using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Repository;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController: ControllerBase
{
    private readonly IUserRepository _userRepository;
    
    public ProfileController(IUserRepository userRepository) {
        _userRepository = userRepository;
    }
    [HttpGet("{username}")]
    
    public Task<User> GetUser(string username) {
        return _userRepository.GetProfile(username);
    }
}