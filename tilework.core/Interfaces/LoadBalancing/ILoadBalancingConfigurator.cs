using Tilework.Persistence.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Interfaces;

public interface ILoadBalancingConfigurator
{
    List<BaseLoadBalancer> LoadConfiguration();
    Task ApplyConfiguration(List<BaseLoadBalancer> loadBalancers);
    Task ApplyConfiguration(BaseLoadBalancer loadBalancer);
    Task Shutdown();
}