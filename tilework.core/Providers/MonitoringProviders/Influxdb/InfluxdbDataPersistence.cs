using System.Globalization;
using System.Reflection;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using AutoMapper;
using InfluxDB3.Client;
using InfluxDB3.Client.Query;
using InfluxDB3.Client.Write;

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
        var host = GetHost();
        return _apiFactory.GetApiService($"{host}/api/v3/");
    }

    private async Task<string> GetHost()
    {
        var container = await GetContainer();
        var host = Host.Parse((await _containerManager.GetContainerAddress(container.Id)).ToString());
        return $"http://{host.Value}:8181";
    }

    private async Task<string> GetAdminToken()
    {
        if (_adminToken != null)
        {
            _logger.LogInformation($"Admin token: {_adminToken}");
            return _adminToken;
        }

        var container = await GetContainer();

        var result = await _containerManager.ExecuteContainerCommand(container.Id, "get_token.sh");

        _adminToken = result.Stdout;
        _logger.LogInformation($"Admin token: {_adminToken}");
        return _adminToken;
    }

    public async Task<List <T>> GetData<T>(string name, DateTimeOffset start, DateTimeOffset end) where T : BaseMonitorData, new()
    {
        using var client = new InfluxDBClient(await GetHost(), token: await GetAdminToken(), database: name);


        var measurementNames = new List<string>();
        await foreach (var row in client.Query(query: "SHOW MEASUREMENTS", queryType: QueryType.InfluxQL))
        {
            measurementNames.Add((string) row[1]);
        }

        var query = $"select * from {measurementNames[0]} WHERE time >= $min_time AND time < $max_time";

        var parameters = new Dictionary<string, object>
        {
            ["min_time"] = start.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["max_time"] = end.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        var entryProperties = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanWrite && p.Name != nameof(BaseMonitorData.Timestamp))
            .ToArray();

        var data = new List<T>();
        await foreach (PointDataValues point in client.QueryPoints(query: query, queryType: QueryType.InfluxQL, namedParameters: parameters))
        {
            var entry = new T();
            entry.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long) point.GetTimestamp() / 1000000);
            foreach (var property in entryProperties)
            {
                var fieldValue = point.GetField(property.Name.ToLower());
                if (fieldValue == null)
                    continue;

                if (TryConvertFieldValue(fieldValue, property.PropertyType, out var convertedValue))
                {
                    property.SetValue(entry, convertedValue);
                }
            }

            data.Add(entry);
        }

        return data;
    }

    private static bool TryConvertFieldValue(object value, Type targetType, out object? convertedValue)
    {
        var destinationType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (destinationType.IsInstanceOfType(value))
        {
            convertedValue = value;
            return true;
        }

        if (value is IConvertible)
        {
            try
            {
                convertedValue = Convert.ChangeType(value, destinationType, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                // Ignore conversion failures so other fields can still be processed.
            }
        }

        convertedValue = null;
        return false;
    }
}
