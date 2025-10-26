using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using AutoMapper;

using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using Tilework.Core.Enums;
using Tilework.Monitoring.Interfaces;
using Tilework.Monitoring.Models;
using Tilework.Monitoring.Enums;

namespace Tilework.Monitoring.Collectd;

public class CollectdConfigurator : IDataCollectorConfigurator
{
    public string ServiceName => "Collectd";

    private readonly IContainerManager _containerManager;
    private readonly DataCollectorConfiguration _settings;
    private readonly ILogger<CollectdConfigurator> _logger;
    private readonly IMapper _mapper;

    public CollectdConfigurator(IOptions<DataCollectorConfiguration> settings,
                               IContainerManager containerManager,
                               ILogger<CollectdConfigurator> logger,
                               IMapper mapper)
    {
        _logger = logger;
        _settings = settings.Value;
        _containerManager = containerManager;
        _mapper = mapper;
    }

    private async Task<Container?> GetContainer()
    {
        var containers = await _containerManager.ListContainers("monitoring.tile");

        return containers.FirstOrDefault(c => c.Name == "DataCollector-collectd");
    }

    private async Task<Container> CreateContainer()
    {
        try
        {
            var container = await _containerManager.CreateContainer(
                "DataCollector-collectd",
                _settings.BackendImage,
                "monitoring.tile",
                new List<ContainerPort>() { }
            );

            return container;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to create container for collectd data collector: {ex.ToString()}");
            throw;
        }
    }

    private void UpdateConfigFile(string path, List<MonitoringSource> sources)
    {
        var config = new Configuration(path);
        config.Load();

        var plugin = new PluginSection()
        {
            Name = "python",
            Imports = ["collectd_haproxy"],
            Modules = sources.Where(s => s.Type == MonitoringSourceType.HAPROXY)
                             .Select(s => (ModuleSection) new HaproxyModuleSection()
                             {
                                Name = "haproxy",
                                Instance = s.Name,
                                Endpoint = $"{s.Host.Value}:{s.Port}"
                             }).ToList()
        };

        config.Plugins.Clear();
        config.Plugins.Add(plugin);

        config.Save();
    }



    public async Task ApplyConfiguration(List<MonitoringSource> sources)
    {
        var container = await GetContainer();
        if (container == null)
            container = await CreateContainer();

        var localConfigPath = Path.GetTempFileName();
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "collectd.conf");

        if (!File.Exists(configPath))
            throw new InvalidOperationException($"No default collectd configuration file found at {configPath}");

        try
        {
            File.Copy(configPath, localConfigPath, overwrite: true);
            UpdateConfigFile(localConfigPath, sources);
            await _containerManager.CopyFileToContainer(container.Id, localConfigPath, "/etc/collectd/collectd.conf");
        }
        finally
        {
            if (File.Exists(localConfigPath))
                File.Delete(localConfigPath);
        }

        if (container.State != ContainerState.Running)
        {
            _logger.LogInformation($"Starting container for data collector");
            await _containerManager.StartContainer(container.Id);
        }
        else
        {
            _logger.LogInformation($"Restarting container for data collector");
            await _containerManager.StopContainer(container.Id);
            await _containerManager.StartContainer(container.Id);
        }
    }

    public async Task Shutdown()
    {
        var container = await GetContainer();
        if (container != null)
        {
            _logger.LogInformation($"Stopping and deleting collectd data collector");
            if (container.State == ContainerState.Running)
                await _containerManager.StopContainer(container.Id);
            await _containerManager.DeleteContainer(container.Id);
        }
    }
}