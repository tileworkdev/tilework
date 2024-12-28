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

    public List<LoadBalancer> LoadConfiguration()
    {
        return null;
    }

    private async Task<List<Container>> GetLoadBalancerContainers()
    {
        return await _containerManager.ListContainers("loadbalancing.tile");
    }

    private void UpdateConfigFile(string path, LoadBalancer config)
    {
        var haproxyConfig = new Configuration(path);
        haproxyConfig.Load();

        List<Listener> listeners = new List<Listener>();
        List<TargetGroup> targetGroups = new List<TargetGroup>();

        foreach(var listener in config.Listeners)
        {
            listeners.Add(listener);
            foreach(var rule in listener.Rules)
            {
                if(!targetGroups.Any(tg => tg.Id == rule.Group.Id))
                    targetGroups.Add(rule.Group);
            }
        }

        haproxyConfig.Frontends = new List<ConfigSection>();
        haproxyConfig.Backends = new List<ConfigSection>();

        foreach(var listener in listeners)
        {
            var fe = ListenerToFrontend.Map(listener);
            haproxyConfig.Frontends.Add(fe);
        }

        foreach(var tg in targetGroups)
        {
            var be = TargetGroupToBackend.Map(tg);
            haproxyConfig.Backends.Add(be);
        }

        haproxyConfig.Save();
    }

    public async Task ApplyConfiguration(List<LoadBalancer> config)
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

    public async Task<bool> CheckLoadBalancerStatus(LoadBalancer balancer)
    {
        var containers = await GetLoadBalancerContainers();
        var container = containers.First(cnt => cnt.Name == balancer.Id.ToString());
        return container.State == ContainerState.Running;
    }
}