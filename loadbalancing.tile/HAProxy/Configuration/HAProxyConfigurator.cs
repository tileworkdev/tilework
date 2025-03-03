using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Interfaces;
using Tilework.LoadBalancing.Settings;

using Tilework.Core.Interfaces;
using Tilework.Core.Enums;
using Tilework.Core.Models;
using SharpCompress;

namespace Tilework.LoadBalancing.Haproxy;

public class HAProxyConfigurator : ILoadBalancingConfigurator
{
    public string ServiceName => "HAProxy";

    private readonly IContainerManager _containerManager;
    private readonly LoadBalancerSettings _settings;
    private readonly ILogger<HAProxyConfigurator> _logger;

    public HAProxyConfigurator(IOptions<LoadBalancerSettings> settings,
                               IContainerManager containerManager,
                               ILogger<HAProxyConfigurator> logger)
    {
        _logger = logger;
        _settings = settings.Value;
        _containerManager = containerManager;
    }

    public List<BaseLoadBalancer> LoadConfiguration()
    {
        return null;
    }

    private async Task<List<Container>> GetLoadBalancerContainers()
    {
        return await _containerManager.ListContainers("loadbalancing.tile");
    }

    private void UpdateConfigFile(string path, BaseLoadBalancer balancer)
    {
        var haproxyConfig = new Configuration(path);
        haproxyConfig.Load();

        haproxyConfig.Frontends = new List<ConfigSection>();
        haproxyConfig.Backends = new List<ConfigSection>();

        var fe = LoadBalancerToFrontend.Map(balancer);
        haproxyConfig.Frontends.Add(fe);

        if (balancer is ApplicationLoadBalancer appLoadBalancer)
        {

        }
        else if(balancer is NetworkLoadBalancer netLoadBalancer)
        {


            var be = TargetGroupToBackend.Map(netLoadBalancer.TargetGroup);
            haproxyConfig.Backends.Add(be);
        }
        else
            throw new ArgumentException("Invalid load balancer type");

        haproxyConfig.Save();
    }

    public async Task ApplyConfiguration(List<BaseLoadBalancer> config)
    {
        if(string.IsNullOrEmpty(_settings.BackendImage))
            throw new ArgumentException("No image setting supplied for load balancing tile");

        var containers = await GetLoadBalancerContainers();
        foreach(var lb in config)
        {
            var name = lb.Id.ToString();
            var container = containers.FirstOrDefault(cnt => cnt.Name == name);

            if(container == null)
            {
                _logger.LogInformation($"Creating new container for load balancer {lb.Name}");
                container = await _containerManager.CreateContainer(name, _settings.BackendImage, "loadbalancing.tile");
            }

            var localConfigPath = Path.GetTempFileName();
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "haproxy.cfg");

            if (!File.Exists(configPath))
                throw new InvalidOperationException($"No default haproxy configuration file found at {configPath}");

            try
            {
                File.Copy(configPath, localConfigPath, overwrite: true);
                UpdateConfigFile(localConfigPath, lb);
                await _containerManager.CopyFileToContainer(container.Id, localConfigPath, "/usr/local/etc/haproxy/haproxy.cfg");
            }
            finally
            {
                if (File.Exists(localConfigPath))
                    File.Delete(localConfigPath);
            }


            if(lb.Enabled == true && container.State != ContainerState.Running)
            {
                _logger.LogInformation($"Starting container for load balancer {lb.Name}");
                await _containerManager.StartContainer(container.Id);
            }
            else if(lb.Enabled == false && container.State == ContainerState.Running)
            {
                _logger.LogInformation($"Stopping container for load balancer {lb.Name}");
                await _containerManager.StopContainer(container.Id);
            }
            else
            {
                _logger.LogInformation($"Signaling container for load balancer {lb.Name} of configuration changes");
                await _containerManager.KillContainer(container.Id, UnixSignal.SIGHUP);
            }
        }

        var containersToDelete = containers.Where(cnt => !config.Any(lb => lb.Id.ToString() == cnt.Name)).ToList();
        foreach(var cnt in containersToDelete)
        {
            _logger.LogInformation($"Deleting defunct load balancer {cnt.Name}");
            if(cnt.State == ContainerState.Running)
                await _containerManager.StopContainer(cnt.Id);
            await _containerManager.DeleteContainer(cnt.Id);
        }
    }

    public async Task<bool> CheckLoadBalancerStatus(BaseLoadBalancer balancer)
    {
        var containers = await GetLoadBalancerContainers();
        var container = containers.First(cnt => cnt.Name == balancer.Id.ToString());
        return container.State == ContainerState.Running;
    }
}