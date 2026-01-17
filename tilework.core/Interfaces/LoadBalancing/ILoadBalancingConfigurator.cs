using Tilework.Persistence.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Interfaces;

public interface ILoadBalancingConfigurator
{
    List<LoadBalancer> LoadConfiguration();
    Task ApplyConfiguration(List<LoadBalancer> loadBalancers);
    Task ApplyConfiguration(LoadBalancer loadBalancer);
    Task Shutdown();
}