using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Tilework.LoadBalancing.Interfaces;
using Tilework.LoadBalancing.Models;
using Tilework.Core.Persistence;
using Tilework.Persistence.LoadBalancing.Models;
using Tilework.LoadBalancing.Haproxy;

namespace Tilework.LoadBalancing.Services;

public class LoadBalancerStatisticsService : ILoadBalancerStatisticsService
{
    private readonly TileworkContext _dbContext;
    private readonly LoadBalancerConfiguration _settings;
    private readonly ILoadBalancingMonitor _monitor;
    private readonly ILogger<LoadBalancerStatisticsService> _logger;

    public LoadBalancerStatisticsService(IServiceProvider serviceProvider,
                                         TileworkContext dbContext,
                                         IOptions<LoadBalancerConfiguration> settings,
                                         ILogger<LoadBalancerStatisticsService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
        _monitor = LoadMonitor(serviceProvider, _settings);
    }

    private ILoadBalancingMonitor LoadMonitor(IServiceProvider serviceProvider, LoadBalancerConfiguration settings)
    {
        return settings.Backend switch
        {
            "haproxy" => serviceProvider.GetRequiredService<HAProxyMonitor>(),
            _ => throw new ArgumentException($"Invalid monitor in load balancing tile: {settings.Backend}")
        };
    }

    public async Task<List<LoadBalancerStatisticsDTO>> GetStatistics(Guid Id, DateTimeOffset start, DateTimeOffset end)
    {
        return await _dbContext.LoadBalancerStatistics
            .AsNoTracking()
            .Where(lbs => lbs.LoadBalancerId == Id && lbs.Timestamp >= start && lbs.Timestamp <= end)
            .Select(s => new LoadBalancerStatisticsDTO()
            {
                Timestamp = s.Timestamp,
                Statistics = s.Statistics
            }).ToListAsync();
    }

    public async Task FetchStatistics(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(Id);
        if (entity == null)
            throw new ArgumentNullException("Non-existent load balancer");

        var statistics = await _monitor.GetRealtimeStatistics(entity);

        var last_statistics = await _dbContext.LoadBalancerStatistics.Where(s => s.LoadBalancerId == Id)
                                                                     .OrderByDescending(s => s.Timestamp)
                                                                     .FirstOrDefaultAsync();

        TimeSpan duration = TimeSpan.Zero;
        string? msg = null;

        var current_timestamp = DateTimeOffset.UtcNow;

        if (last_statistics != null)
        {
            // If duration between timestamps is bigger than monitoring interval, assume that
            // monitoring stopped for some time. Restart monitoring
            if (current_timestamp - last_statistics.Timestamp > TimeSpan.FromSeconds(60 + 10))
            {
                msg = "monitoring interrupted";
                duration = TimeSpan.Zero;
            }

            duration = statistics.Uptime - last_statistics.Statistics.Uptime;

            // If current uptime is smaller than previous, there was a restart
            if (duration < TimeSpan.Zero)
            {
                msg = "smaller uptime";
                duration = TimeSpan.Zero;
            }

            // If duration between uptimes and duration between timestamps have a variation of more than +-10s
            // there was a restart
            if (Math.Abs((last_statistics.Timestamp + duration - current_timestamp).TotalSeconds) > 10)
            {
                msg = "duration variation";
                duration = TimeSpan.Zero;
            }
        }
        else
        {
            msg = "no prev statistics";
        }

        if (duration == TimeSpan.Zero && msg != null)
        {
            _logger.LogDebug($"Detected load balancer {Id} restart during collection of statistics ({msg}). Restarting statistics collection");
        }

        var statisticsPoint = new LoadBalancerStatistics()
        {
            Duration = duration,
            LoadBalancer = entity,
            Timestamp = current_timestamp,
            Statistics = (duration != TimeSpan.Zero && last_statistics != null) ? statistics - last_statistics.Statistics : statistics
        };

        await _dbContext.LoadBalancerStatistics.AddAsync(statisticsPoint);
        await _dbContext.SaveChangesAsync();
    }
}

