using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using AutoMapper;

using Tomlyn;
using Tomlyn.Model;

using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using Tilework.Core.Enums;
using Tilework.Monitoring.Interfaces;
using Tilework.Monitoring.Models;
using Tilework.Monitoring.Enums;

namespace Tilework.Monitoring.Telegraf;

public class TelegrafConfigurator : IDataCollectorConfigurator
{
    public string ServiceName => "Telegraf";

    private string ContainerName => $"DataCollector-{ServiceName}";

    private readonly IContainerManager _containerManager;
    private readonly DataCollectorConfiguration _settings;
    private readonly ILogger<TelegrafConfigurator> _logger;
    private readonly IMapper _mapper;

    public TelegrafConfigurator(IOptions<DataCollectorConfiguration> settings,
                               IContainerManager containerManager,
                               ILogger<TelegrafConfigurator> logger,
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
                new List<ContainerPort>() { }
            );

            return container;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Failed to create container for telegraf data collector: {ex.ToString()}");
            throw;
        }
    }

    private static T GetOrCreate<T>(TomlTable parent, string name)
        where T : class, new()
    {
        if (parent.TryGetValue(name, out var obj) && obj is T t)
            return t;

        var created = new T();
        parent[name] = created;
        return created;
    }

    private void UpdateConfigFile(string path, List<Monitoring.Models.Monitor> monitors)
    {
        var text = File.ReadAllText(path);
        var config = Toml.ToModel(text);

        if (monitors.Count() > 0)
        {
            var inputs = GetOrCreate<TomlTable>(config, "inputs");
            var outputs = GetOrCreate<TomlTable>(config, "outputs");

            foreach (var monitor in monitors)
            {
                var source = monitor.Source;

                switch (source.Type)
                {
                    case MonitoringSourceType.HAPROXY:
                    {
                        var array = GetOrCreate<TomlTableArray>(inputs, "haproxy");

                        array.Add(new TomlTable
                        {
                            ["servers"] = new TomlArray {
                                $"tcp://{source.Host.Value}:{source.Port}"
                            },
                            ["interval"] = "30s",
                            ["tags"] = new TomlTable {
                                ["instance"] = source.Name
                            }
                        });

                        break;
                    }

                    default:
                        break;
                }

                var target = monitor.Target;

                switch(target.Type)
                {
                    case MonitoringPersistenceType.INFLUXDB:
                    {
                        var array = GetOrCreate<TomlTableArray>(outputs, "influxdb_v2");

                        array.Add(new TomlTable
                        {
                            ["urls"] = new TomlArray { $"http://{target.Host.Value}:{target.Port}" },
                            ["token"] = target.Password,
                            ["bucket"] = source.Module,
                            ["organization"] = "tilework",
                            ["tagpass"] = new TomlTable
                            {
                                ["instance"] = new TomlArray { source.Name }
                            }
                        });

                        break;
                    }

                    default:
                        break;
                }
            }
        }


        text = Toml.FromModel(config);
        File.WriteAllText(path, text);
    }


    public async Task ApplyConfiguration(List<Monitoring.Models.Monitor> monitors)
    {
        var container = await GetContainer();

        if (monitors.Count() == 0)
        {
            _logger.LogInformation("No active monitors found. Deferring configuration for data collector");
            if (container != null)
            {
                _logger.LogInformation($"Stopping container for data collector");
                await _containerManager.StopContainer(container.Id);
            }
            return;
        }

        
        if (container == null)
            container = await CreateContainer();

        var localConfigPath = Path.GetTempFileName();
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "telegraf.conf");

        if (!File.Exists(configPath))
            throw new InvalidOperationException($"No default telegraf configuration file found at {configPath}");

        try
        {
            File.Copy(configPath, localConfigPath, overwrite: true);
            UpdateConfigFile(localConfigPath, monitors);
            await _containerManager.CopyFileToContainer(container.Id, localConfigPath, "/etc/telegraf/telegraf.conf");
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
            _logger.LogInformation($"Stopping and deleting telegraf data collector");
            if (container.State == ContainerState.Running)
                await _containerManager.StopContainer(container.Id);
            await _containerManager.DeleteContainer(container.Id);
        }
    }
}