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
using Tilework.TokenVault.Services;

namespace Tilework.Monitoring.Influxdb;

public class Influxdb3Configurator : BaseContainerProvider, IDataPersistenceConfigurator
{
    protected static string _serviceName = "influxdb";
    protected static string _moduleName = "monitoring";

    protected static List<ContainerPort> _ports = new List<ContainerPort>()
    {
        new ContainerPort()
        {
            Port = 8181,
            HostPort = 8181,
            Type = PortType.TCP
        }
    };

    private readonly IContainerManager _containerManager;
    private readonly DataPersistenceConfiguration _settings;
    private readonly ILogger<Influxdb3Configurator> _logger;
    private readonly IMapper _mapper;
    private readonly HttpApiFactoryService _apiFactory;
    private readonly TokenService _tokenService;

    public Influxdb3Configurator(IOptions<DataPersistenceConfiguration> settings,
                                 IContainerManager containerManager,
                                 ILogger<Influxdb3Configurator> logger,
                                 TokenService tokenService,
                                 HttpApiFactoryService httpApiFactoryService,
                                 IMapper mapper) : base(containerManager, logger, _moduleName, _serviceName, settings.Value.BackendImage, _ports)
    {
        _logger = logger;
        _settings = settings.Value;
        _containerManager = containerManager;
        _mapper = mapper;
        _apiFactory = httpApiFactoryService;
        _tokenService = tokenService;
    }

    public async Task<MonitoringTarget> GetTarget(MonitoringSource source)
    {
        var container = await GetContainer();

        return new MonitoringTarget()
        {
            Name = _serviceName,
            Type = Enums.MonitoringPersistenceType.INFLUXDB,
            Host = Host.Parse((await _containerManager.GetContainerAddress(container.Id)).ToString()),
            Port = 8181,
            Password = await GetAdminToken()
        };
    }

    public async Task ApplyConfiguration()
    {
        await StartUp();
    }

    private async Task<HttpApiService> GetApiService()
    {
        var host = await GetHost();
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
        var container = await GetContainer();

        var tokenKey = $"influxdb.{container.Id}";

        var token = await _tokenService.GetToken(tokenKey);

        if(token == null)
        {
            _logger.LogInformation("Generating a new admin token for influxdb3");

            var result = await _containerManager.ExecuteContainerCommand(container.Id, $"influxdb3 create token --admin");
            token = result.Stdout;

            await _tokenService.SetToken(tokenKey, token);
        }
        return token;
    }

    public async Task<List <T>> GetData<T>(string module, Dictionary<string, string> filters, TimeSpan interval, DateTimeOffset start, DateTimeOffset end) where T : BaseMonitorData, new()
    {
        // This method has not been maintained and tested. Disable it for now
        throw new NotImplementedException();


        using var client = new InfluxDBClient(await GetHost(), token: await GetAdminToken(), database: module);


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
