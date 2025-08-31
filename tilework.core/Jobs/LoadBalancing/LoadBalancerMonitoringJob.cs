using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Tilework.LoadBalancing.Interfaces;

namespace Tilework.Core.Jobs.LoadBalancing;

public class LoadBalancerMonitoringJob : IInvocable
{
    private readonly ILoadBalancerService _loadBalancerService;
    private readonly ILoadBalancerStatisticsService _statisticsService;
    private readonly ILogger<LoadBalancerMonitoringJob> _logger;
    public LoadBalancerMonitoringJob(ILoadBalancerService loadBalancerService,
                                     ILoadBalancerStatisticsService statisticsService,
                                     ILogger<LoadBalancerMonitoringJob> logger)
    {
        _loadBalancerService = loadBalancerService;
        _statisticsService = statisticsService;
        _logger = logger;
    }

    public async Task Invoke()
    {
        var balancers = await _loadBalancerService.GetLoadBalancers();

        _logger.LogInformation($"Fetching load balancer statistics");
        foreach (var balancer in balancers.Where(b => b.Enabled == true))
        {
            try
            {
                await _statisticsService.FetchStatistics(balancer.Id);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to fetch statistics for balancer {balancer.Id}: {ex.Message}");
            }
        }
    }
}
