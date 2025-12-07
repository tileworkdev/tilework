using System.Globalization;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using AutoMapper;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;

using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using Tilework.Core.Enums;

using Tilework.Monitoring.Interfaces;
using Tilework.Monitoring.Models;
using Tilework.Core.Services;
using Tilework.TokenVault.Services;

namespace Tilework.Monitoring.Influxdb;

public class Influxdb2Configurator : BaseContainerProvider, IDataPersistenceConfigurator
{
    protected static string _serviceName = "influxdb";
    protected static string _moduleName = "monitoring";

    private static string _orgName = "tilework";

    protected static List<ContainerPort> _ports = new List<ContainerPort>()
    {
        new ContainerPort()
        {
            Port = 8086,
            HostPort = 8086,
            Type = PortType.TCP
        }
    };


    private readonly IContainerManager _containerManager;
    private readonly DataPersistenceConfiguration _settings;
    private readonly ILogger<Influxdb2Configurator> _logger;
    private readonly IMapper _mapper;
    private readonly HttpApiFactoryService _apiFactory;
    private readonly TokenService _tokenService;

    public Influxdb2Configurator(IOptions<DataPersistenceConfiguration> settings,
                                 IContainerManager containerManager,
                                 ILogger<Influxdb2Configurator> logger,
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

        await CheckCreateBucket(_orgName, source.Module);

        return new MonitoringTarget()
        {
            Name = _serviceName,
            Type = Enums.MonitoringPersistenceType.INFLUXDB,
            Host = Host.Parse((await _containerManager.GetContainerAddress(container.Id)).ToString()),
            Port = 8086,
            Password = await GetAdminToken()
        };
    }

    public async Task ApplyConfiguration()
    {
        var container = await GetContainer();
        if(container == null || container.State != ContainerState.Running)
            await StartUp();

        await CheckRunSetup();
    }

    private async Task CheckRunSetup()
    {
        var service = await GetApiService();
        for(int i=0; i<5; i++)
        {
            try
            {
                var resp = await service.ApiGet<SetupResponse>("/setup");

                if(resp.Allowed == true)
                {
                    var container = await GetContainer();
                    var tokenKey = $"influxdb.{container.Id}";

                    await _tokenService.DeleteToken(tokenKey);

                    await GetAdminToken();
                }
                break;
            }
            catch
            {
                await Task.Delay(2000);
                continue;
            }
        }
    }



    private async Task<HttpApiService> GetApiService()
    {
        var host = await GetHost();
        return _apiFactory.GetApiService($"{host}/api/v2/");
    }

    private async Task<string> GetHost()
    {
        var container = await GetContainer();
        var host = Host.Parse((await _containerManager.GetContainerAddress(container.Id)).ToString());
        return $"http://{host.Value}:8086";
    }

    private async Task<string> GetAdminToken()
    {
        var container = await GetContainer();

        var tokenKey = $"influxdb.{container.Id}";

        var token = await _tokenService.GetToken(tokenKey);

        if(token == null)
        {
            _logger.LogInformation("Generating a new admin token for influxdb2");
            token = TokenService.GenerateToken(16);

            var result = await _containerManager.ExecuteContainerCommand(
                container.Id,
                $"influx setup --username admin --password \"{token}\" --org \"{_orgName}\" --bucket tilework --token \"{token}\" --force");

            await _tokenService.SetToken(tokenKey, token);
        }
        _logger.LogInformation($"Admin token ---> {token}");
        return token;
    }

    private async Task CheckCreateBucket(string orgName, string bucketName)
    {
        using var client = new InfluxDBClient(await GetHost(), token: await GetAdminToken());
        var api = client.GetBucketsApi();

        var buckets = await api.FindBucketsByOrgNameAsync(orgName);
        var bucket = buckets.FirstOrDefault(b => b.Name == bucketName);

        if (bucket == null)
        {
            var orgId = await GetOrgId(orgName);

            await api.CreateBucketAsync(
                    name: bucketName,
                    orgId: orgId,
                    bucketRetentionRules: new BucketRetentionRules(
                        type: BucketRetentionRules.TypeEnum.Expire,
                        everySeconds: 30 * 24 * 3600
                    )
            );
        }
    }

    private async Task<string> GetOrgId(string orgName)
    {
        using var client = new InfluxDBClient(await GetHost(), token: await GetAdminToken());

        var orgsApi = client.GetOrganizationsApi();
        var org = await orgsApi.FindOrganizationsAsync(org:orgName);
        if(org.Count() == 0)
            throw new ArgumentException("Invalid organisation name");

        return org[0].Id;
    }

    public async Task<List <T>> GetData<T>(string module, Dictionary<string, string> filters, TimeSpan interval, DateTimeOffset start, DateTimeOffset end) where T : BaseMonitorData, new()
    {
        using var client = new InfluxDBClient(await GetHost(), token: await GetAdminToken());

        var queryApi = client.GetQueryApi();

        var startStr = start.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);
        var stopStr = end.UtcDateTime.ToString("o", CultureInfo.InvariantCulture);

        var query = $"from(bucket: \"{module}\")\n  |> range(start: {startStr}, stop: {stopStr})";

        if (filters is { Count: > 0 })
        {
            var filterExpressions = filters.Select(filter =>
            {
                var key = (filter.Key ?? string.Empty).Replace("\"", "\\\"");
                var value = (filter.Value ?? string.Empty).Replace("\"", "\\\"");
                return $"r[\"{key}\"] == \"{value}\"";
            });

            var filtersCombined = string.Join(" and ", filterExpressions);
            query += $"\n  |> filter(fn: (r) => {filtersCombined})";
        }

        query += $"  |> aggregateWindow(every: {ToFluxDuration(interval)}, fn: sum, createEmpty: true)";



        var fluxTables = await queryApi.QueryAsync(query, _orgName);

        var entryProperties = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanWrite && p.Name != nameof(BaseMonitorData.Timestamp))
            .ToArray();

        var entryPropertyNames = entryProperties
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var records = fluxTables[0].Records
            .Where(record => record.GetField() is string fieldName && entryPropertyNames.Contains(fieldName))
            .ToList();

        var groups = records
            .Where(record => record.GetTimeInDateTime().HasValue)
            .GroupBy(record => record.GetTimeInDateTime()!.Value)
            .OrderBy(group => group.Key)
            .ToList();

        var data = new List<T>();

        foreach(var group in groups)
        {
            var entry = new T();
            entry.Timestamp = new DateTimeOffset(DateTime.SpecifyKind(group.Key, DateTimeKind.Utc));

            foreach (var property in entryProperties)
            {
                var fieldValue = group.FirstOrDefault(r => r.GetField().ToLower() == property.Name.ToLower())?.GetValue();
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

    private string ToFluxDuration(TimeSpan ts)
    {
        var parts = new List<string>();

        if (ts.Days > 0) parts.Add($"{ts.Days}d");
        if (ts.Hours > 0) parts.Add($"{ts.Hours}h");
        if (ts.Minutes > 0) parts.Add($"{ts.Minutes}m");
        if (ts.Seconds > 0) parts.Add($"{ts.Seconds}s");
        if (ts.Milliseconds > 0) parts.Add($"{ts.Milliseconds}ms");

        // If everything is zero, return "0s"
        return parts.Count > 0 ? string.Join("", parts) : "0s";
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
