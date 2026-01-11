namespace Tilework.Core.Interfaces;

public interface ICommand
{
    string Name { get; }
    Task<int> run(string[] args);
}