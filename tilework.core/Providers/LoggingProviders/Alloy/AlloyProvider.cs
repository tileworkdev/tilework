using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using Tilework.Core.Enums;
using Tilework.Core.Services;
using Tilework.Logging.Interfaces;
using Tilework.Logging.Models;

namespace Tilework.Logging.Alloy;

public class AlloyConfigurator : BaseContainerProvider, ILoggingDataCollectorConfigurator
{
    protected static string _serviceName = "alloy";
    protected static string _moduleName = "logging";
    protected static string _defaultName = "default";

    private const string DockerSocketPath = "/var/run/docker.sock";

    private readonly ILogger<AlloyConfigurator> _logger;

    public AlloyConfigurator(IOptions<LoggingDataCollectorConfiguration> settings,
                             IContainerManager containerManager,
                             ILogger<AlloyConfigurator> logger)
        : base(containerManager, logger, _moduleName, _serviceName, settings.Value.BackendImage)
    {
        _logger = logger;
    }

    public async Task ApplyConfiguration(List<LoggingSource> sources, LoggingTarget target)
    {
        if (sources.Count == 0)
        {
            _logger.LogInformation("No active logging sources found. Deferring configuration for data collector");
            await Shutdown();
            return;
        }

        var localConfigPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            BuildConfiguration(sources, target).Save(localConfigPath);

            var containerFile = new ContainerFile
            {
                LocalPath = localConfigPath,
                ContainerPath = "/etc/alloy/config.alloy"
            };

            var dockerSocket = new ContainerMount
            {
                Source = DockerSocketPath,
                Target = DockerSocketPath,
                ReadOnly = true
            };

            await StartUp(_defaultName, new(), new() { containerFile },
                ContainerRestartType.RESTART, new() { dockerSocket });
        }
        finally
        {
            if (File.Exists(localConfigPath))
                File.Delete(localConfigPath);
        }
    }

    public async Task Shutdown()
    {
        await Shutdown(_defaultName);
    }

    private static Configuration BuildConfiguration(
        IReadOnlyList<LoggingSource> sources,
        LoggingTarget target)
    {
        var configuration = new Configuration();
        configuration
            .AddBlock("loki.write", "default")
            .AddBlock("endpoint")
            .Set("url", $"http://{target.Host.Value}:{target.Port}/loki/api/v1/push");

        var dockerHost = $"unix://{DockerSocketPath}";
        for (var index = 0; index < sources.Count; index++)
        {
            var source = sources[index];
            var componentName = $"source_{index}";

            configuration
                .AddBlock("discovery.docker", componentName)
                .Set("host", dockerHost)
                .AddBlock("filter")
                .Set("name", "id")
                .SetArray("values", ConfigValue.String(source.ContainerId));

            configuration
                .AddBlock("loki.source.docker", componentName)
                .Set("host", dockerHost)
                .SetRaw("targets", $"discovery.docker.{componentName}.targets")
                .SetMap("labels", new Dictionary<string, string>
                {
                    ["module"] = source.Module,
                    ["instance"] = source.Name,
                    ["container"] = source.ContainerName
                })
                .SetArray("forward_to", ConfigValue.Raw("loki.write.default.receiver"));
        }

        return configuration;
    }
}
