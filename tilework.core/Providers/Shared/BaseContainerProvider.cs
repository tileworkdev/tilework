using Microsoft.Extensions.Logging;


using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using Tilework.Core.Enums;
using Tilework.Logging.Models;
using Tilework.Logging.Services;

namespace Tilework.Core.Services;

public abstract class BaseContainerProvider
{
    private readonly ILogger _logger;
    private readonly IContainerManager _containerManager;

    private readonly string _module;
    private readonly string _service;
    private readonly string _imageName;
    private readonly LoggingDataCollectorService? _loggingService;

    private string _fullModule => $"{_module}.tile";
    

    public BaseContainerProvider(IContainerManager containerManager,
                                 ILogger logger,
                                 string module,
                                 string service,
                                 string imageName,
                                 LoggingDataCollectorService? loggingService = null)
    {
        _containerManager = containerManager;
        _logger = logger;

        _module = module;
        _service = service;
        _imageName = imageName;
        _loggingService = loggingService;

        if (string.IsNullOrEmpty(_imageName))
            throw new ArgumentException($"No image setting supplied for {_module}.{_service}");
    }

    protected string GetFullName(string name)
    {
        return $"{_module}.{_service}.{name}";
    }

    private bool IsFullName(string name)
    {
        var parts = name.Split('.');
        return parts.Length == 3 &&
               parts[0] == _module &&
               parts[1] == _service;
    }

    protected async Task<List<Container>> GetContainers()
    {
        return await _containerManager.ListContainers(_fullModule);
    }

    protected async Task<Container?> GetContainer(string name)
    {
        var containers = await GetContainers();
        return containers.FirstOrDefault(c => c.Name == (IsFullName(name) ? name : GetFullName(name)));
    }

    protected async Task ConfigureContainerLogging(string containerName)
    {
        if (_loggingService == null)
            throw new InvalidOperationException(
                $"No logging service was supplied to provider {_module}.{_service}");

        var container = await GetContainer(containerName)
            ?? throw new InvalidOperationException(
                $"Cannot configure logging because container {GetFullName(containerName)} does not exist");

        await _loggingService.StartLogging(new LoggingSource
        {
            Module = _module,
            Name = containerName,
            ContainerId = container.Id,
            ContainerName = container.Name
        });
    }

    private async Task<Container> CreateContainer(string name, List<ContainerPort> ports,
                                                  List<ContainerMount>? mounts)
    {
        try
        {
            var container = await _containerManager.CreateContainer(
                GetFullName(name), _imageName, _fullModule, ports, mounts
            );

            return container;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to create container {GetFullName(name)}: {ex}");
            throw;
        }
    }

    protected async Task StartUp(string name, List<ContainerPort> ports, List<ContainerFile> files,
                                 ContainerRestartType restartType,
                                 List<ContainerMount>? mounts = null)
    {
        var container = await GetContainer(name);

        if(container != null)
        {
            var existingPorts = await _containerManager.GetContainerPorts(container.Id);

            if (ComparePorts(existingPorts, ports))
            {
                _logger.LogInformation($"Container {GetFullName(name)} ports changed, forcing recreate");
                restartType = ContainerRestartType.RECREATE;
            }

            var existingMounts = await _containerManager.GetContainerMounts(container.Id);
            if (CompareMounts(existingMounts, mounts))
            {
                _logger.LogInformation($"Container {GetFullName(name)} mounts changed, forcing recreate");
                restartType = ContainerRestartType.RECREATE;
            }
        }


        if(container != null && restartType == ContainerRestartType.RECREATE)
        {
            await DeleteContainer(name);
            container = null;
        }

        if (container == null)
        {
            _logger.LogInformation($"Creating container {GetFullName(name)}");
            container = await CreateContainer(name, ports, mounts);
        }

        foreach(var file in files)
        {
            await _containerManager.CopyFileToContainer(container.Id, file.LocalPath, file.ContainerPath);
        }


        if (container.State != ContainerState.Running)
        {
            _logger.LogInformation($"Starting container {GetFullName(name)}");
            await _containerManager.StartContainer(container.Id);
        }
        else
        {
            if(restartType == ContainerRestartType.RESTART)
            {
                _logger.LogInformation($"Restarting container {GetFullName(name)}");
                await _containerManager.StopContainer(container.Id);
                await _containerManager.StartContainer(container.Id);
            }
            else
            {
                _logger.LogInformation($"Signaling container {GetFullName(name)} of configuration changes");
                await _containerManager.KillContainer(container.Id, UnixSignal.SIGHUP);
            }
        }
    }

    private static bool ComparePorts(List<ContainerPort>? existingPorts, List<ContainerPort>? desiredPorts)
    {
        var normalizedExisting = NormalizePorts(existingPorts);
        var normalizedDesired = NormalizePorts(desiredPorts);

        if (normalizedExisting.Count != normalizedDesired.Count)
            return true;

        for (int i = 0; i < normalizedExisting.Count; i++)
        {
            var existing = normalizedExisting[i];
            var desired = normalizedDesired[i];

            if (existing.Port != desired.Port ||
                existing.HostPort != desired.HostPort ||
                existing.Type != desired.Type)
            {
                return true;
            }
        }

        return false;
    }

    private static List<ContainerPort> NormalizePorts(List<ContainerPort>? ports)
    {
        if (ports == null)
            return new List<ContainerPort>();

        return ports
            .OrderBy(p => p.Port)
            .ThenBy(p => p.HostPort ?? -1)
            .ThenBy(p => p.Type)
            .ToList();
    }

    private static bool CompareMounts(List<ContainerMount>? existingMounts,
                                      List<ContainerMount>? desiredMounts)
    {
        var normalizedDesired = NormalizeMounts(desiredMounts);
        var normalizedExisting = NormalizeMounts(existingMounts);

        if (normalizedExisting.Count != normalizedDesired.Count)
            return true;

        return normalizedDesired.Where((mount, index) => mount != normalizedExisting[index]).Any();
    }

    private static List<(string Source, string Target, bool ReadOnly)> NormalizeMounts(
        List<ContainerMount>? mounts)
    {
        return mounts?
            .Select(mount => (mount.Source, mount.Target, mount.ReadOnly))
            .OrderBy(mount => mount.Source, StringComparer.Ordinal)
            .ThenBy(mount => mount.Target, StringComparer.Ordinal)
            .ToList() ?? new List<(string Source, string Target, bool ReadOnly)>();
    }

    private async Task DeleteContainer(string name)
    {
        var container = await GetContainer(name);
        if (container != null)
        {
            if (container.State == ContainerState.Running)
            {
                _logger.LogInformation($"Stopping container {container.Name}");
                await _containerManager.StopContainer(container.Id);
            }
            _logger.LogInformation($"Deleting container {container.Name}");
            await _containerManager.DeleteContainer(container.Id);
        }
    }

    public async Task Shutdown(string name)
    {
        if (_loggingService != null)
        {
            var container = await GetContainer(name);
            if (container != null)
                await _loggingService.StopLoggingForContainer(container.Name);
        }

        await DeleteContainer(name);
    }
}
