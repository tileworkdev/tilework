using Tilework.LoadBalancing.Persistence.Models;

namespace Tilework.LoadBalancing.Interfaces;

public interface ILoadBalancingConfigurator
{
    string ServiceName { get; }

    List<LoadBalancer> LoadConfiguration();
    Task ApplyConfiguration(List<LoadBalancer> config);
}