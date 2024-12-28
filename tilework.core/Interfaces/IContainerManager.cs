namespace Tilework.Core.Interfaces;

using Tilework.Core.Enums;
using Tilework.Core.Models;

public interface IContainerManager
{
    public Task<List<Container>> ListContainers(string? module);
    public Task<Container> CreateContainer(string name, string image, string module);
    public Task DeleteContainer(string id);
    public Task CopyFileToContainer(string id, string localPath, string containerPath);

    public Task StartContainer(string id);
    public Task StopContainer(string id);
    public Task KillContainer(string id, UnixSignal signal);
}