using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

using Docker.DotNet;
using Docker.DotNet.Models;

using Tilework.Core.Enums;
using Tilework.Core.Interfaces;
using Tilework.Core.Models;

namespace Tilework.Core.Services;

public class DockerServiceManager : IContainerManager
{
    private readonly ILogger<DockerServiceManager> _logger;
    private readonly DockerClient _client;
    public DockerServiceManager(ILogger<DockerServiceManager> logger)
    {
        _logger = logger;
        _client = new DockerClientConfiguration().CreateClient();
    }

    public async Task<List<Container>> ListContainers()
    {
        var containers = await _client.Containers.ListContainersAsync(
            new ContainersListParameters(){
                All=true
            });

        return containers.Select(cnt => new Container {
            Id = cnt.ID,
            Name = cnt.Names[0]
        }).ToList();
    }

    public async Task<Container> CreateContainer(string name, string image)
    {
        var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters()
        {
            Image = image,
            Labels = new Dictionary<string, string> {
                {"TileworkManaged", "true"}
            }
        });

        await _client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters());

        return (await ListContainers()).FirstOrDefault(cnt => cnt.Id == response.ID);
    }

    public async Task DeleteContainer(string id)
    {
        await _client.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters()
        {
            RemoveVolumes = true
        });
    }
}