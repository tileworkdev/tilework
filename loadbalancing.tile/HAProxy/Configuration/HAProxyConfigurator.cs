using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Interfaces;

namespace Tilework.LoadBalancing.Haproxy;

public class HAProxyConfigurator : ILoadBalancingConfigurator
{
    public string ServiceName => "HAProxy";

    public List<LoadBalancer> LoadConfiguration()
    {
        return null;
    }

    public void ApplyConfiguration(List<LoadBalancer> config)
    {
        var haproxyConfig = new Configuration("/etc/haproxy/haproxy.cfg");
        haproxyConfig.Load();

        foreach(var lb in config)
        {
            var be = TargetGroupToBackend.Map(lb.Group);
            var fe = LoadBalancerToFrontend.Map(lb);

            

            haproxyConfig.Frontends.Add(fe);
            haproxyConfig.Backends.Add(be);
        }

        haproxyConfig.Save();
    }
}