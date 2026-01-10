using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using AutoMapper;

using Tilework.Persistence.IdentityManagement.Models;
using Tilework.IdentityManagement.Models;

namespace Tilework.IdentityManagement.Services;

public class UserService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    public UserService(UserManager<User> userManager,
                       RoleManager<Role> roleManager,
                       IMapper mapper,
                       ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<UserDTO>> GetUsers()
    {
        var users = await _userManager.Users.ToListAsync();

        return _mapper.Map<List<UserDTO>>(users);
    }

}