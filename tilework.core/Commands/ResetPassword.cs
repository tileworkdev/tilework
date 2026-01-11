using Microsoft.Extensions.Logging;

using Tilework.Core.Interfaces;
using Tilework.IdentityManagement.Services;

namespace Tilework.Core.Commands;

public class ResetPasswordCommand : ICommand
{
    static string _name = "ResetPassword";
    public string Name { get => _name; }

    private readonly ILogger<ResetPasswordCommand> _logger;
    private readonly UserService _userService;

    public ResetPasswordCommand(ILogger<ResetPasswordCommand> logger, UserService userService)
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
        string? userName = args.Length > 1 ? args[1] : null;
        while (string.IsNullOrEmpty(userName))
        {
            Console.Write("Username: ");
            userName = Console.ReadLine();
        }

        string? password = null;
        while (string.IsNullOrEmpty(password))
        {
            Console.Write("Password: ");
            password = ReadPassword();
        }

        string? passwordRepeat = null;
        while (string.IsNullOrEmpty(passwordRepeat))
        {
            Console.Write("Password (repeat): ");
            passwordRepeat = ReadPassword();
        }

        if (password != passwordRepeat)
        {
            Console.WriteLine("Passwords do not match");
            return -1;
        }

        try
        {
            var user = await _userService.GetUserByLogin(userName);
            if (user == null)
            {
                Console.WriteLine($"User \"{userName}\" not found");
                return -1;
            }

            await _userService.ResetPassword(user.Id, password);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Password reset failed for {UserName}", userName);
            Console.WriteLine($"Password reset failed: {ex}");
            return -1;
        }

        Console.WriteLine("Password reset successfully");
        return 0;
    }
}
