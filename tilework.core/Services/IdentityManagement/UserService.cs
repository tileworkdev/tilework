using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

    public async Task<UserDTO?> GetUser(Guid Id)
    {
        var user = await _userManager.FindByIdAsync(Id.ToString());

        return user != null ? _mapper.Map<UserDTO>(user) : null;
    }

    public async Task<UserDTO> AddUser(UserDTO userDto, string password)
    {
        if (userDto == null)
        {
            throw new ArgumentNullException(nameof(userDto));
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        var user = new User
        {
            UserName = userDto.UserName,
            Email = userDto.Email,
            Active = userDto.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            _logger.LogWarning("Failed to create user {UserName}: {Errors}", userDto.UserName, errors);
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        return _mapper.Map<UserDTO>(user);
    }

    public async Task<UserDTO> UpdateUser(UserDTO userDto)
    {
        if (userDto == null)
        {
            throw new ArgumentNullException(nameof(userDto));
        }

        var user = await _userManager.FindByIdAsync(userDto.Id.ToString());
        if (user == null)
        {
            throw new ArgumentException($"User {userDto.Id} not found.");
        }

        user.UserName = userDto.UserName;
        user.Email = userDto.Email;
        user.Active = userDto.Active;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            _logger.LogWarning("Failed to update user {UserId}: {Errors}", userDto.Id, errors);
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }

        return _mapper.Map<UserDTO>(user);
    }

    public async Task DeleteUser(Guid Id)
    {
        var user = await _userManager.FindByIdAsync(Id.ToString());
        if (user == null)
        {
            return;
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            _logger.LogWarning("Failed to delete user {UserId}: {Errors}", Id, errors);
            throw new InvalidOperationException($"Failed to delete user: {errors}");
        }
    }

    public async Task<User?> GetUserByLogin(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            return null;
        }

        var user = await _userManager.FindByNameAsync(login)
                   ?? await _userManager.FindByEmailAsync(login);
        return user;
    }

    public async Task UpdateLastLogin(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.LastLoginAtUtc = DateTimeOffset.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            _logger.LogWarning("Failed to update last login for user {UserId}: {Errors}", user.Id, errors);
            throw new InvalidOperationException($"Failed to update last login: {errors}");
        }
    }

}
