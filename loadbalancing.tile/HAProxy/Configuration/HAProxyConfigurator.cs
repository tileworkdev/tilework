using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Interfaces;

using Tilework.Core.Interfaces;

namespace Tilework.LoadBalancing.Haproxy;

public class HAProxyConfigurator : ILoadBalancingConfigurator
{
    public string ServiceName => "HAProxy";

    private readonly IServiceManager _serviceManager;

    public HAProxyConfigurator(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public List<LoadBalancer> LoadConfiguration()
    {
        return null;
    }

    public void ApplyConfiguration(List<LoadBalancer> config)
    {
        var haproxyConfig = new Configuration("/etc/haproxy/haproxy.cfg");
        haproxyConfig.Load();

        List<LoadBalancer> loadBalancers = new List<LoadBalancer>();
        List<TargetGroup> targetGroups = new List<TargetGroup>();

        foreach(var lb in config)
        {
            loadBalancers.Add(lb);
            if(!targetGroups.Any(tg => tg.Id == lb.Group.Id))
                targetGroups.Add(lb.Group);
        }

        haproxyConfig.Frontends = new List<ConfigSection>();
        haproxyConfig.Backends = new List<ConfigSection>();

        foreach(var lb in loadBalancers)
        {
            var fe = LoadBalancerToFrontend.Map(lb);
            haproxyConfig.Frontends.Add(fe);
        }

        foreach(var tg in targetGroups)
        {
            var be = TargetGroupToBackend.Map(tg);
            haproxyConfig.Backends.Add(be);
        }

        haproxyConfig.Save();

        if(config.Count() == 0)
            _serviceManager.StopService("haproxy");
        else
            _serviceManager.RestartService("haproxy");
    }
}