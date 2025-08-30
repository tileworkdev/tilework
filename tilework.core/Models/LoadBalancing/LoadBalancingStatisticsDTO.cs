namespace Tilework.LoadBalancing.Models;

public class LoadBalancerStatisticsDTO
{
    public DateTimeOffset Timestamp { get; set; }
    public LoadBalancingStatistics Statistics { get; set; }
}