using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using Tilework.Core.Enums;
using Tilework.Logging.Interfaces;
using Tilework.Logging.Models;
using Tilework.Logging.Enums;
using Tilework.Core.Services;

namespace Tilework.Logging.Loki;

public class LokiConfigurator : BaseContainerProvider, ILoggingDataPersistenceConfigurator
{
    protected static readonly string _serviceName = "loki";
    protected static readonly string _moduleName = "logging";
    private static readonly string _defaultName = "default";


#if DEBUG
    protected static readonly List<ContainerPort> _ports = new List<ContainerPort>()
    {
        new ContainerPort()
        {
            Port = 3100,
            HostPort = 3100,
            Type = PortType.TCP
        }
    };
#else
    protected static readonly List<ContainerPort> _ports = new List<ContainerPort>() {};
#endif

    private readonly IContainerManager _containerManager;
    private readonly HttpApiFactoryService _apiFactory;

    public LokiConfigurator(IOptions<LoggingDataPersistenceConfiguration> settings,
                            IContainerManager containerManager,
                            HttpApiFactoryService apiFactory,
                            ILogger<LokiConfigurator> logger)
        : base(containerManager, logger, _moduleName, _serviceName, settings.Value.BackendImage)
    {
        _containerManager = containerManager;
        _apiFactory = apiFactory;
    }

    public async Task<LoggingTarget> GetTarget()
    {
        var container = await GetContainer(_defaultName)
            ?? throw new InvalidOperationException("Loki is not configured");
        var address = await _containerManager.GetContainerAddress(container.Id)
            ?? throw new InvalidOperationException("Loki does not have a network address");

        return new LoggingTarget
        {
            Name = _defaultName,
            Type = LoggingPersistenceType.LOKI,
            Host = Host.Parse(address.ToString()),
            Port = 3100
        };
    }

    public async Task ApplyConfiguration()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "loki.yaml");

        if (!File.Exists(configPath))
            throw new InvalidOperationException($"No default Loki configuration file found at {configPath}");

        var container = await GetContainer(_defaultName);
        if (container == null || container.State != ContainerState.Running)
        {
            var containerFile = new ContainerFile
            {
                LocalPath = configPath,
                ContainerPath = "/etc/loki/local-config.yaml"
            };

            await StartUp(_defaultName, _ports, new() { containerFile }, ContainerRestartType.RESTART);
        }
    }

    public async Task<List<LoggingData>> GetData(
        string module,
        Dictionary<string, string> filters,
        DateTimeOffset start,
        DateTimeOffset end,
        SortOrder order)
    {
        if (string.IsNullOrWhiteSpace(module))
            throw new ArgumentException("A logging module is required", nameof(module));
        ArgumentNullException.ThrowIfNull(filters);
        if (end < start)
            throw new ArgumentException("The end timestamp must not be before the start timestamp", nameof(end));

        var selectors = new Dictionary<string, string>(filters, StringComparer.Ordinal);
        selectors["module"] = module;

        var query = "{" + string.Join(",", selectors.Select(selector =>
        {
            ValidateLabelName(selector.Key);
            return $"{selector.Key}=\"{EscapeLabelValue(selector.Value)}\"";
        })) + "}";

        var target = await GetTarget();
        var api = _apiFactory.GetApiService($"http://{target.Host.Value}:{target.Port}");
        var response = await api.ApiGet<QueryRangeResponse>(
            "/loki/api/v1/query_range",
            query: new Dictionary<string, string>
            {
                ["query"] = query,
                ["start"] = ToUnixNanoseconds(start),
                ["end"] = ToUnixNanoseconds(end),
                ["direction"] = order == SortOrder.Ascending ? "forward" : "backward",
                ["limit"] = "5000"
            });

        if (!string.Equals(response.Status, "success", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Loki returned an unsuccessful query response");
        if (response.Data == null || response.Data.Result.Count == 0)
            return new List<LoggingData>();
        if (!string.Equals(response.Data.ResultType, "streams", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Unexpected Loki result type: {response.Data.ResultType}");

        var entries = response.Data.Result
            .SelectMany(stream => stream.Values
                .Where(value => value.Count >= 2)
                .Select(value => new LoggingData
                {
                    Timestamp = FromUnixNanoseconds(value[0].GetString()),
                    Message = value[1].GetString() ?? string.Empty,
                    Labels = new Dictionary<string, string>(stream.Labels, StringComparer.Ordinal)
                }))
            .ToList();

        return order == SortOrder.Ascending
            ? entries.OrderBy(entry => entry.Timestamp).ToList()
            : entries.OrderByDescending(entry => entry.Timestamp).ToList();
    }

    public async Task Shutdown()
    {
        await Shutdown(_defaultName);
    }

    private static string ToUnixNanoseconds(DateTimeOffset timestamp)
    {
        var utcTicks = timestamp.UtcTicks - DateTimeOffset.UnixEpoch.UtcTicks;
        return checked(utcTicks * 100).ToString();
    }

    private static DateTimeOffset FromUnixNanoseconds(string? value)
    {
        if (!long.TryParse(value, out var nanoseconds))
            throw new FormatException($"Invalid Loki timestamp: {value}");

        return DateTimeOffset.UnixEpoch.AddTicks(nanoseconds / 100);
    }

    private static void ValidateLabelName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            !(char.IsLetter(name[0]) || name[0] == '_') ||
            name.Skip(1).Any(character => !(char.IsLetterOrDigit(character) || character == '_')))
        {
            throw new ArgumentException($"Invalid Loki label name: {name}", nameof(name));
        }
    }

    private static string EscapeLabelValue(string value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\"", "\\\"");
    }
}
