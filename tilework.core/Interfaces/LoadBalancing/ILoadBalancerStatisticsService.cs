using Tilework.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Interfaces;

public interface ILoadBalancerStatisticsService
{
    public Task<List<LoadBalancerStatisticsDTO>> GetStatistics(Guid Id, DateTimeOffset start, DateTimeOffset end);
    public Task FetchStatistics(Guid Id);
}

