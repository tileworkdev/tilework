using Tilework.Core.Interfaces;

namespace Tilework.Core.Commands;

public class PrintVersionInfoCommand : ICommand
{
    static string _name = "Version";
    public string Name { get => _name; }

    public PrintVersionInfoCommand()
    {
    }

    public async Task<int> run(string[] args)
    {
        Console.WriteLine(AppMetadata.InformationalVersion);
        return 0;
    }
}
