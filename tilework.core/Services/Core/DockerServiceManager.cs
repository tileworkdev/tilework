using System.Net;
using System.Text;

using Microsoft.Extensions.Logging;

using Docker.DotNet;
using Docker.DotNet.Models;
using SharpCompress.Common;
using SharpCompress.Writers;

using Tilework.Core.Enums;
using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using Tilework.Exceptions.Core;


namespace Tilework.Core.Services;

public class DockerServiceManager : IContainerManager
{
    private static string defaultNetworkName = "tileworknet";
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

    public async Task Initialize()
    {
        var network = await GetOrCreateDefaultNetwork();
        await AddMeToDefaultNetwork(network);
    }

    private async Task AddMeToDefaultNetwork(ContainerNetwork network)
    {
        var containerId = Dns.GetHostName();

        ContainerInspectResponse container;
        try {
            container = await _client.Containers.InspectContainerAsync(containerId);
        }
        catch (DockerContainerNotFoundException)
        {
            _logger.LogInformation("Not adding tilework to default network: Not containerized");
            return;
        }

        var attachedNetworks = container.NetworkSettings?.Networks ?? new Dictionary<string, EndpointSettings>();

        var alreadyInNetwork = attachedNetworks.Any(n =>
            string.Equals(n.Key, network.Name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(n.Value.NetworkID, network.Id, StringComparison.OrdinalIgnoreCase));


        if(!alreadyInNetwork)
        {
            await _client.Networks.ConnectNetworkAsync(network.Id, new NetworkConnectParameters
            {
                Container = containerId
            });
        }
    }

    private async Task<ContainerNetwork> GetOrCreateDefaultNetwork()
    {
        var networks = await ListNetworks();
        var network = networks.FirstOrDefault(net => net.Name == defaultNetworkName);

        if (network == null)
            network = await CreateNetwork(defaultNetworkName);

        return network;
    }

    public async Task<List<ContainerNetwork>> ListNetworks()
    {
        var labelFilters = new Dictionary<string, bool>
        {
            { "dev.tilework.managed=true", true }
        };

        var networks = await _client.Networks.ListNetworksAsync(
            new NetworksListParameters
            {
                Filters = new Dictionary<string, IDictionary<string, bool>> {
                    { "label", labelFilters }
                }
            }
        );

        return networks.Select(net => new ContainerNetwork
        {
            Id = net.ID,
            Name = net.Name
        }).ToList();
    }


    public async Task<ContainerNetwork> CreateNetwork(string name)
    {
        var tags = new Dictionary<string, string> {
            {"dev.tilework.managed", "true"}
        };

        var response = await _client.Networks.CreateNetworkAsync(
            new NetworksCreateParameters
            {
                Name = name,
                Driver = "bridge",
                Labels = tags
            });

        return (await ListNetworks()).First(net => net.Id == response.ID);
    }


    public async Task DeleteNetwork(string id)
    {
        await _client.Networks.DeleteNetworkAsync(id);
    }

    public async Task<IPAddress?> GetContainerAddress(string id)
    {

        var info = await _client.Containers.InspectContainerAsync(id);

        if (info.NetworkSettings.Networks.Count == 0)
            return null;

        if (info.NetworkSettings.Networks.Count > 1)
            _logger.LogWarning("Container is attached on multiple networks. Getting address on first");

        var network = info.NetworkSettings.Networks.First();

        return IPAddress.Parse(network.Value.IPAddress);
    }


    public async Task<List<Container>> ListContainers(string? module = null)
    {
        var labelFilters = new Dictionary<string, bool>
        {
            { "dev.tilework.managed=true", true }
        };

        if (!string.IsNullOrEmpty(module))
            labelFilters.Add($"dev.tilework.module={module}", true);


        var containers = await _client.Containers.ListContainersAsync(
            new ContainersListParameters()
            {
                All = true,
                Filters = new Dictionary<string, IDictionary<string, bool>> {
                    { "label", labelFilters }
                }
            });

        return containers.Select(cnt => new Container
        {
            Id = cnt.ID,
            Name = cnt.Names[0].TrimStart('/'),
            State = ParseState(cnt.State)
        }).ToList();
    }

    private async Task<bool> ImageExists(string image)
    {
        var images = await _client.Images.ListImagesAsync(new ImagesListParameters { All = true });
        return images.Any(img => img.RepoTags?.Contains(image) == true);
    }

    public async Task<Container> CreateContainer(string name, string image, string module, List<ContainerPort>? ports)
    {
        string[] imageParts = image.Split(':');

        if ((await ImageExists(image)) == false)
        {
            _logger.LogInformation($"Cannot find image {image} locally. Pulling it");

            await _client.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = imageParts[0],
                    Tag = imageParts[1],
                },
                null,
                new Progress<JSONMessage>()
            );
        }



        var tags = new Dictionary<string, string> {
            {"dev.tilework.managed", "true"},
            {"dev.tilework.module", module}
        };

        var exposedPorts = new Dictionary<string, EmptyStruct>();
        var portBindings = new Dictionary<string, IList<PortBinding>>();


        if (ports != null)
        {
            foreach (var port in ports)
            {
                string portKey = $"{port.Port}/{port.Type.ToString().ToLower()}";
                exposedPorts[portKey] = default;

                if (port.HostPort.HasValue)
                {
                    portBindings[portKey] = new List<PortBinding>
                    {
                        new PortBinding { HostPort = port.HostPort.Value.ToString() }
                    };
                }
            }
        }

        var network = await GetOrCreateDefaultNetwork();

        var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters()
        {
            Image = image,
            Name = name,
            Labels = tags,
            ExposedPorts = exposedPorts,
            HostConfig = new HostConfig
            {
                RestartPolicy = new RestartPolicy
                {
                    Name = RestartPolicyKind.UnlessStopped,
                },
                PortBindings = portBindings
            },
            NetworkingConfig = new NetworkingConfig
            {
                EndpointsConfig = new Dictionary<string, EndpointSettings>
                {
                    [network.Name] = new EndpointSettings()
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

            using (var tarStream = File.OpenRead(tempTarPath))
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
        try
        {
            await _client.Containers.StartContainerAsync(id, new ContainerStartParameters());
        }
        catch (DockerApiException ex)
        {
            throw new DockerException(ex.ResponseBody);
        }

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


    public async Task<ContainerCommandResult> ExecuteContainerCommand(string id, string command)
    {
        var execCreate = await _client.Exec.ExecCreateContainerAsync(id, new ContainerExecCreateParameters
        {
            AttachStdout = true,
            AttachStderr = true,
            Cmd = ["sh", "-c", command]
        });


        using var stream = await _client.Exec.StartAndAttachContainerExecAsync(execCreate.ID, false);


        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        var buffer = new byte[8192];
        MultiplexedStream.ReadResult result;
        do
        {
            result = await stream.ReadOutputAsync(buffer, 0, buffer.Length, default);
            if (result.EOF) break;

            var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
            if (result.Target == MultiplexedStream.TargetStream.StandardOut)
                stdout.Append(text);
            else
                stderr.Append(text);
        }
        while (!result.EOF);

        var execInspect = await _client.Exec.InspectContainerExecAsync(execCreate.ID);

        return new ContainerCommandResult()
        {
            ExitCode = (int)execInspect.ExitCode,
            Stdout = stdout.ToString(),
            Stderr = stderr.ToString()
        };
    }
}