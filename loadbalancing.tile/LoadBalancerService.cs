using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


using Tilework.LoadBalancing.Persistence;
using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Interfaces;
using Tilework.LoadBalancing.Settings;
using Tilework.LoadBalancing.Enums;

using Tilework.LoadBalancing.Haproxy;


namespace Tilework.LoadBalancing.Services;


public class LoadBalancerService
{
    private readonly LoadBalancerContext _dbContext;
    private readonly LoadBalancerSettings _settings;
    private readonly ILoadBalancingConfigurator _configurator;
    private readonly ILogger<LoadBalancerService> _logger;
    

    public LoadBalancerService(IServiceProvider serviceProvider,
                               LoadBalancerContext dbContext,
                               IOptions<LoadBalancerSettings> settings,
                               ILogger<LoadBalancerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
        _configurator = LoadConfigurator(serviceProvider, _settings);
        
        ApplyConfiguration().GetAwaiter().GetResult();
    }

    private ILoadBalancingConfigurator LoadConfigurator(IServiceProvider serviceProvider, LoadBalancerSettings settings)
    {
        return settings.Backend switch
        {
            "haproxy" => serviceProvider.GetRequiredService<HAProxyConfigurator>(),
            _ => throw new ArgumentException($"Invalid configurator in load balancing tile: {_settings.Backend}")
        };
    }

    public async Task<List<BaseLoadBalancer>> GetLoadBalancers()
    {
        return await _dbContext.LoadBalancers.ToListAsync();
    }

    public async Task<BaseLoadBalancer?> GetLoadBalancer(Guid Id)
    {
        return await _dbContext.LoadBalancers.FindAsync(Id);
    }

    public async Task AddLoadBalancer(BaseLoadBalancer balancer)
    {
        await _dbContext.LoadBalancers.AddAsync(balancer);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateLoadBalancer(BaseLoadBalancer balancer)
    {
        _dbContext.LoadBalancers.Update(balancer);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteLoadBalancer(BaseLoadBalancer balancer)
    {
        _dbContext.LoadBalancers.Remove(balancer);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<TargetGroup>> GetTargetGroups()
    {
        return await _dbContext.TargetGroups.ToListAsync();
    }

    public async Task<List<TargetGroup>> GetNlbTargetGroups()
    {
        var protocols = new List<TargetGroupProtocol> {
            TargetGroupProtocol.TCP,
            TargetGroupProtocol.UDP,
            TargetGroupProtocol.TCP_UDP,
            TargetGroupProtocol.TLS
        };
        return (await GetTargetGroups()).Where(tg => protocols.Contains(tg.Protocol)).ToList();
    }

    public async Task<List<TargetGroup>> GetAlbTargetGroups()
    {
        var protocols = new List<TargetGroupProtocol> {
            TargetGroupProtocol.HTTP,
            TargetGroupProtocol.HTTPS
        };
        return (await GetTargetGroups()).Where(tg => protocols.Contains(tg.Protocol)).ToList();
    }

    public async Task<TargetGroup?> GetTargetGroup(Guid Id)
    {
        return await _dbContext.TargetGroups.FindAsync(Id);
    }

    public async Task AddTargetGroup(TargetGroup group)
    {
        await _dbContext.TargetGroups.AddAsync(group);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateTargetGroup(TargetGroup group)
    {
        _dbContext.TargetGroups.Update(group);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteTargetGroup(TargetGroup group)
    {
        _dbContext.TargetGroups.Remove(group);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ApplyConfiguration()
    {
        var balancers = await _dbContext.LoadBalancers.ToListAsync();
        await _configurator.ApplyConfiguration(balancers);
    }
}
