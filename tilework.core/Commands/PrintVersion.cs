using Tilework.Core.Interfaces;
using System.Reflection;

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
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        Console.WriteLine(informationalVersion);
        return 0;
    }
}