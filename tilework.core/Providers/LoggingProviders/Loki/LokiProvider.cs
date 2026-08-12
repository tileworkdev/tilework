using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using Tilework.Core.Enums;
using Tilework.Logging.Interfaces;
using Tilework.Logging.Models;
using Tilework.Core.Services;

namespace Tilework.Logging.Loki;

public class LokiConfigurator : BaseContainerProvider, ILoggingDataPersistenceConfigurator
{
    protected static string _serviceName = "loki";
    protected static string _moduleName = "logging";
    private static string _defaultName = "default";

    protected static List<ContainerPort> _ports = new()
    {
        new ContainerPort()
        {
            Port = 3100,
            HostPort = 3100,
            Type = PortType.TCP
        }
    };

    private readonly IContainerManager _containerManager;

    public LokiConfigurator(IOptions<LoggingDataPersistenceConfiguration> settings,
                            IContainerManager containerManager,
                            ILogger<LokiConfigurator> logger)
        : base(containerManager, logger, _moduleName, _serviceName, settings.Value.BackendImage)
    {
        _containerManager = containerManager;
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
            Host = Host.Parse(address.ToString()),
            Port = 3100
        };
    }

    public async Task ApplyConfiguration()
    {
        var container = await GetContainer(_defaultName);
        if (container == null || container.State != ContainerState.Running)
            await StartUp(_defaultName, _ports, new(), ContainerRestartType.RESTART);
    }

    public async Task Shutdown()
    {
        await Shutdown(_defaultName);
    }
}
