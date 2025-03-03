using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

using Docker.DotNet;
using Docker.DotNet.Models;
using SharpCompress.Common;
using SharpCompress.Writers;

using Tilework.Core.Enums;
using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using System.Runtime.InteropServices;

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

    public static Enums.ContainerState ParseState(string state)
    {
        if (Enum.TryParse(state, true, out Enums.ContainerState result))
        {
            return result;
        }

        throw new ArgumentException($"Invalid container state: {state}");
    }

    public async Task<List<Container>> ListContainers(string? module = null)
    {
        var labelFilters = new Dictionary<string, bool>
        {
            { "TileworkManaged=true", true }
        };

        if (!string.IsNullOrEmpty(module))
            labelFilters.Add($"Module={module}", true);


        var containers = await _client.Containers.ListContainersAsync(
            new ContainersListParameters() {
                All=true,
                Filters = new Dictionary<string, IDictionary<string, bool>> {
                    { "label", labelFilters }
                }
            });

        return containers.Select(cnt => new Container {
            Id = cnt.ID,
            Name = cnt.Names[0].TrimStart('/'),
            State = ParseState(cnt.State)
        }).ToList();
    }

    public async Task<Container> CreateContainer(string name, string image, string module)
    {
        string[] imageParts = image.Split(':');

        await _client.Images.CreateImageAsync(
            new ImagesCreateParameters
            {
                FromImage = imageParts[0],
                Tag = imageParts[1],
            },
            null,
            new Progress<JSONMessage>());


        var tags = new Dictionary<string, string> {
            {"TileworkManaged", "true"},
            {"Module", module}
        };

        var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters()
        {
            Image = image,
            Name = name,
            Labels = tags,
            HostConfig = new HostConfig
            {
                RestartPolicy = new RestartPolicy
                {
                    Name = RestartPolicyKind.UnlessStopped
                }
            }
        });

        return (await ListContainers()).First(cnt => cnt.Id == response.ID);
    }

    public async Task DeleteContainer(string id)
    {
        await _client.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters()
        {
            RemoveVolumes = true,
            Force = true
        });
    }

    public async Task CopyFileToContainer(string id, string localPath, string containerPath)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Container ID cannot be null or empty.", nameof(id));
        if (string.IsNullOrEmpty(localPath))
            throw new ArgumentException("Local path cannot be null or empty.", nameof(localPath));
        if (string.IsNullOrEmpty(containerPath))
            throw new ArgumentException("Container path cannot be null or empty.", nameof(containerPath));

        if (!File.Exists(localPath))
            throw new FileNotFoundException($"The file at path '{localPath}' does not exist.");


        string tempTarPath = Path.GetTempFileName();

        try
        {
            using (var tarStream = File.Create(tempTarPath))
            using (var tarWriter = WriterFactory.Open(tarStream, ArchiveType.Tar, CompressionType.None))
            {
                tarWriter.Write(
                    containerPath.TrimStart('/'),
                    File.OpenRead(localPath),
                    File.GetLastWriteTimeUtc(localPath)
                );
            }

            using(var tarStream = File.OpenRead(tempTarPath))
            {
                await _client.Containers.ExtractArchiveToContainerAsync(
                    id,
                    new ContainerPathStatParameters { Path = "/" },
                    tarStream);
            }

            _logger.LogInformation($"File {localPath} successfully copied to container {id} in path {containerPath}");
        }
        finally
        {
            if (File.Exists(tempTarPath))
                File.Delete(tempTarPath);
        }
    }


    public async Task StartContainer(string id)
    {
        await _client.Containers.StartContainerAsync(id, new ContainerStartParameters());
    }

    public async Task StopContainer(string id)
    {
        await _client.Containers.StopContainerAsync(id, new ContainerStopParameters());
    }

    public async Task KillContainer(string id, UnixSignal signal)
    {
        await _client.Containers.KillContainerAsync(id, new ContainerKillParameters()
        {
            Signal = DockerSignalMapper.MapUnixSignalToDockerSignal(signal)
        });
    }
}