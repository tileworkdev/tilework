using System.Net;

using Tilework.Core.Enums;
using Tilework.Core.Models;

namespace Tilework.Core.Interfaces;

public interface IContainerManager
{
    public Task<List<ContainerNetwork>> ListNetworks();
    public Task<ContainerNetwork> CreateNetwork(string name);
    public Task DeleteNetwork(string id);

    public Task<IPAddress> GetContainerAddress(string id);

    public Task<List<Container>> ListContainers(string? module);
    public Task<Container> CreateContainer(string name, string image, string module, List<ContainerPort>? ports);
    public Task DeleteContainer(string id);
    public Task CopyFileToContainer(string id, string localPath, string containerPath);

    public Task StartContainer(string id);
    public Task StopContainer(string id);
    public Task KillContainer(string id, UnixSignal signal);
}