using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using AutoMapper;

using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using Tilework.Core.Enums;

using Tilework.Monitoring.Interfaces;
using Tilework.Monitoring.Models;
using Tilework.Core.Services;

namespace Tilework.Monitoring.Influxdb;

public class InfluxdbConfigurator : IDataPersistenceConfigurator
{
    public string ServiceName => "Influxdb";

    private string ContainerName => $"DataPersistence-{ServiceName}";

    private readonly IContainerManager _containerManager;
    private readonly DataPersistenceConfiguration _settings;
    private readonly ILogger<InfluxdbConfigurator> _logger;
    private readonly IMapper _mapper;
    private readonly HttpApiFactoryService _apiFactory;

    private string? _adminToken = null;

    public InfluxdbConfigurator(IOptions<DataPersistenceConfiguration> settings,
                               IContainerManager containerManager,
                               ILogger<InfluxdbConfigurator> logger,
                               HttpApiFactoryService httpApiFactoryService,
                               IMapper mapper)
    {
        _logger = logger;
        _settings = settings.Value;
        _containerManager = containerManager;
        _mapper = mapper;
        _apiFactory = httpApiFactoryService;
    }

    private async Task<Container?> GetContainer()
    {
        var containers = await _containerManager.ListContainers("monitoring.tile");

        return containers.FirstOrDefault(c => c.Name == ContainerName);
    }

    private async Task<Container> CreateContainer()
    {
        try
        {
            var container = await _containerManager.CreateContainer(
                ContainerName,
                _settings.BackendImage,
                "monitoring.tile",
                new List<ContainerPort>()
                {
                    new ContainerPort()
                    {
                        Port = 8181,
                        HostPort = 8181,
                        Type = PortType.TCP
                    }
                }
            );

            return container;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to create container for influxdb data persistence: {ex.ToString()}");
            throw;
        }
    }

    public async Task<MonitoringTarget> GetTarget(MonitoringSource source)
    {
        var container = await GetContainer();

        return new MonitoringTarget()
        {
            Name = ServiceName,
            Type = Enums.MonitoringPersistenceType.INFLUXDB,
            Host = Host.Parse((await _containerManager.GetContainerAddress(container.Id)).ToString()),
            Port = 8181,
            Password = await GetAdminToken()
        };
    }

    public async Task ApplyConfiguration()
    {
        var container = await GetContainer();
        if (container == null)
        {
            _logger.LogInformation($"Creating container for influxdb data persistence");
            container = await CreateContainer();
            _logger.LogInformation($"Starting container for influxdb data persistence");
            await _containerManager.StartContainer(container.Id);

            await _containerManager.ExecuteContainerCommand(container.Id, "influxdb3 show tokens --format json");    
        }


        if (container.State != ContainerState.Running)
        {
            _logger.LogInformation($"Starting container for influxdb data persistence");
            await _containerManager.StartContainer(container.Id);
        }
        else
        {
            _logger.LogInformation($"Restarting container for influxdb data persistence");
            await _containerManager.StopContainer(container.Id);
            await _containerManager.StartContainer(container.Id);
        }

        await GetAdminToken();
    }

    public async Task Shutdown()
    {
        var container = await GetContainer();
        if (container != null)
        {
            _logger.LogInformation($"Stopping and deleting influxdb data persistence");
            if (container.State == ContainerState.Running)
                await _containerManager.StopContainer(container.Id);
            await _containerManager.DeleteContainer(container.Id);
        }
    }

    private async Task<HttpApiService> GetApiService()
    {
        var container = await GetContainer();
        var host = Host.Parse((await _containerManager.GetContainerAddress(container.Id)).ToString());

        return _apiFactory.GetApiService($"http://{host.Value}:8181/api/v3/");
    }

    private async Task<string> GetAdminToken()
    {
        if (_adminToken != null)
            return _adminToken;

        var container = await GetContainer();

        var result = await _containerManager.ExecuteContainerCommand(container.Id, "get_token.sh");

        _adminToken = result.Stdout;
        return _adminToken;
    }
}
