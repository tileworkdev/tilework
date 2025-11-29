using Microsoft.Extensions.Logging;


using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using Tilework.Core.Enums;


using Tilework.Core.Models;

public abstract class BaseContainerProvider
{
    private readonly ILogger _logger;
    private readonly IContainerManager _containerManager;

    private readonly string _module;
    private readonly string _name;
    private readonly string _imageName;
    private readonly List<ContainerPort> _ports;

    private string _fullName => $"{_module}.{_name}";
    private string _fullModule => $"{_module}.tile";
    

    public BaseContainerProvider(IContainerManager containerManager,
                                 ILogger logger,
                                 string module,
                                 string name,
                                 string imageName,
                                 List<ContainerPort> ports)
    {
        _containerManager = containerManager;
        _logger = logger;

        _name = name;
        _module = module;
        _imageName = imageName;
        _ports = ports;
    }

    protected async Task<Container?> GetContainer()
    {
        var containers = await _containerManager.ListContainers(_fullModule);

        return containers.FirstOrDefault(c => c.Name == _fullName);
    }

    private async Task<Container> CreateContainer()
    {
        try
        {
            var container = await _containerManager.CreateContainer(
                _fullName, _imageName, _fullModule, _ports
            );

            return container;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to create container {_fullName}: {ex}");
            throw;
        }
    }

    protected async Task StartUp()
    {
        var container = await GetContainer();
        if (container == null)
        {
            _logger.LogInformation($"Creating container {_fullName}");
            container = await CreateContainer();
        }


        if (container.State != ContainerState.Running)
        {
            _logger.LogInformation($"Starting container {_fullName}");
            await _containerManager.StartContainer(container.Id);
        }
        else
        {
            _logger.LogInformation($"Restarting container {_fullName}");
            await _containerManager.StopContainer(container.Id);
            await _containerManager.StartContainer(container.Id);
        }
    }

    public async Task Shutdown()
    {
        var container = await GetContainer();
        if (container != null)
        {
            _logger.LogInformation($"Stopping and deleting container {_fullName}");
            if (container.State == ContainerState.Running)
                await _containerManager.StopContainer(container.Id);
            await _containerManager.DeleteContainer(container.Id);
        }
    }
}