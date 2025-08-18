using Tilework.Persistence.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Interfaces;

public interface ILoadBalancingConfigurator
{
    string ServiceName { get; }

    List<BaseLoadBalancer> LoadConfiguration();
    Task ApplyConfiguration(List<BaseLoadBalancer> config);
    Task Shutdown();
}