using Tilework.LoadBalancing.Models;
using Tilework.Persistence.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Interfaces;

public interface ILoadBalancingMonitor
{
    public Task<LoadBalancingStatistics> GetRealtimeStatistics(BaseLoadBalancer balancer);
}