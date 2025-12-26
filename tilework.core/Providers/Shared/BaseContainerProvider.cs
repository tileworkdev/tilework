using Microsoft.Extensions.Logging;


using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using Tilework.Core.Enums;


public abstract class BaseContainerProvider
{
    private readonly ILogger _logger;
    private readonly IContainerManager _containerManager;

    private readonly string _module;
    private readonly string _service;
    private readonly string _imageName;

    private string _fullModule => $"{_module}.tile";
    

    public BaseContainerProvider(IContainerManager containerManager,
                                 ILogger logger,
                                 string module,
                                 string service,
                                 string imageName)
    {
        _containerManager = containerManager;
        _logger = logger;

        _module = module;
        _service = service;
        _imageName = imageName;

        if (string.IsNullOrEmpty(_imageName))
            throw new ArgumentException($"No image setting supplied for {_module}.{_service}");
    }

    private string getFullName(string name)
    {
        return $"{_module}.{_service}.{name}";
    }

    protected async Task<List<Container>> GetContainers()
    {
        return await _containerManager.ListContainers(_fullModule);
    }

    protected async Task<Container?> GetContainer(string name)
    {
        var containers = await GetContainers();
        return containers.FirstOrDefault(c => c.Name == getFullName(name));
    }

    private async Task<Container> CreateContainer(string name, List<ContainerPort> ports)
    {
        try
        {
            var container = await _containerManager.CreateContainer(
                getFullName(name), _imageName, _fullModule, ports
            );

            return container;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to create container {getFullName(name)}: {ex}");
            throw;
        }
    }

    protected async Task StartUp(string name, List<ContainerPort> ports, List<ContainerFile> files, ContainerRestartType restartType)
    {
        var container = await GetContainer(name);

        if(container != null && restartType == ContainerRestartType.RECREATE)
        {
            await DeleteContainer(name);
            container = null;
        }

        if (container == null)
        {
            _logger.LogInformation($"Creating container {getFullName(name)}");
            container = await CreateContainer(name, ports);
        }

        foreach(var file in files)
        {
            await _containerManager.CopyFileToContainer(container.Id, file.LocalPath, file.ContainerPath);
        }


        if (container.State != ContainerState.Running)
        {
            _logger.LogInformation($"Starting container {getFullName(name)}");
            await _containerManager.StartContainer(container.Id);
        }
        else
        {
            if(restartType == ContainerRestartType.RESTART)
            {
                _logger.LogInformation($"Restarting container {getFullName(name)}");
                await _containerManager.StopContainer(container.Id);
                await _containerManager.StartContainer(container.Id);
            }
            else
            {
                _logger.LogInformation($"Signaling container {getFullName(name)} of configuration changes");
                await _containerManager.KillContainer(container.Id, UnixSignal.SIGHUP);
            }
        }
    }

    private async Task DeleteContainer(string name)
    {
        var container = await GetContainer(name);
        if (container != null)
        {
            _logger.LogInformation($"Stopping and deleting container {getFullName(name)}");
            if (container.State == ContainerState.Running)
                await _containerManager.StopContainer(container.Id);
            await _containerManager.DeleteContainer(container.Id);
        }
    }

    public async Task Shutdown(string name)
    {
        await DeleteContainer(name);
    }
}