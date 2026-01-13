using Microsoft.Extensions.Logging;

using Tilework.IdentityManagement.Models;
using Tilework.IdentityManagement.Services;
using Tilework.Core.Interfaces;

namespace Tilework.Core.Commands;

public class CreateUserCommand : ICommand
{
    static string _name = "CreateUser";
    public string Name { get => _name; }

    private readonly ILogger<CreateUserCommand> _logger;
    private readonly UserService _userService;

    
    public CreateUserCommand(ILogger<CreateUserCommand> logger, UserService userService)
    {
        _logger = logger;
        _userService = userService;
    }


    private string ReadPassword()
    {
        string text = "";
        ConsoleKeyInfo ch = Console.ReadKey(true);
        while (ch.Key != ConsoleKey.Enter)
        {
            text += ch.KeyChar;
            Console.Write('*');
            ch = Console.ReadKey(true);
        }
        Console.WriteLine();
        return text;
    }

    public async Task<int> run(string[] args)
    {
        string? userName = null;
        while(string.IsNullOrEmpty(userName)) {
            Console.Write("Username: ");
            userName = Console.ReadLine();
        }

        string? email = null;
        while(string.IsNullOrEmpty(email)) {
            Console.Write("Email: ");
            email = Console.ReadLine();
        }

        string? password = null;
        while(string.IsNullOrEmpty(password)) {
            Console.Write("Password: ");
            password = ReadPassword();
        }

        string? passwordRepeat = null;
        while(string.IsNullOrEmpty(passwordRepeat)) {
            Console.Write("Password (repeat): ");
            passwordRepeat = ReadPassword();
        }

        if(password != passwordRepeat) 
        {
            Console.WriteLine("Passwords do not match");
            return -1;
        }

        var user = new UserDTO()
        {
            UserName = userName,
            Email = email,
            Active = true
        };

        try 
        {
            await _userService.AddUser(user, password);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"User creation failed: {ex}");
            return -1;
        }

        Console.WriteLine("User created successfully");
        return 0;
    }
}