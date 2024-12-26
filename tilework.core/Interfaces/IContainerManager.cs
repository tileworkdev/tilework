namespace Tilework.Core.Interfaces;

using Tilework.Core.Models;

public interface IContainerManager
{
    public Task<List<Container>> ListContainers();
    public Task<Container> CreateContainer(string name, string image);
    public Task DeleteContainer(string id);
}