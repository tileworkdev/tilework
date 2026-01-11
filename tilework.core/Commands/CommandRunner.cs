using Microsoft.Extensions.DependencyInjection;

using Tilework.Core.Interfaces;

namespace Tilework.Core.Commands;

public class CommandRunner
{
    private readonly IServiceProvider _serviceProvider;

    public CommandRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No command specified");
            return -1;
        }

        var services = _serviceProvider.GetServices<ICommand>();

        ICommand? command;
        try
        {
            command = services.First(o => o.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"Command \"{args[0]}\" not found");
            return -1;
        }

        return await command.run(args);
    }
}
