using Tilework.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Interfaces;

public interface ILoadBalancingConfigurator
{
    string ServiceName { get; }

    List<LoadBalancer> LoadConfiguration();
    void ApplyConfiguration(List<LoadBalancer> config);
}