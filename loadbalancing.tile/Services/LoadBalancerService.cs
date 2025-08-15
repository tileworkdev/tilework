using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using AutoMapper;


using Tilework.LoadBalancing.Persistence;
using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Interfaces;
using Tilework.LoadBalancing.Settings;
using Tilework.LoadBalancing.Haproxy;

using Tilework.Core.Interfaces;
using Tilework.Core.LoadBalancing.Models;
using Tilework.Core.LoadBalancing.Enums;


namespace Tilework.LoadBalancing.Services;


public class LoadBalancerService : ILoadBalancerService
{
    private readonly LoadBalancerContext _dbContext;
    private readonly LoadBalancerSettings _settings;
    private readonly ILoadBalancingConfigurator _configurator;
    private readonly ILogger<LoadBalancerService> _logger;
    private readonly IMapper _mapper;
    private readonly ICertificateManagementService _certificateManagementService;


    public LoadBalancerService(IServiceProvider serviceProvider,
                               LoadBalancerContext dbContext,
                               IMapper mapper,
                               ICertificateManagementService certificateManagementService,
                               IOptions<LoadBalancerSettings> settings,
                               ILogger<LoadBalancerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
        _configurator = LoadConfigurator(serviceProvider, _settings);

        _certificateManagementService = certificateManagementService;
        _mapper = mapper;
    }

    private ILoadBalancingConfigurator LoadConfigurator(IServiceProvider serviceProvider, LoadBalancerSettings settings)
    {
        return settings.Backend switch
        {
            "haproxy" => serviceProvider.GetRequiredService<HAProxyConfigurator>(),
            _ => throw new ArgumentException($"Invalid configurator in load balancing tile: {_settings.Backend}")
        };
    }

    public async Task<List<BaseLoadBalancerDTO>> GetLoadBalancers()
    {
        var entities = await _dbContext.LoadBalancers.ToListAsync();
        return entities.Select(e => 
            e switch
            {
                ApplicationLoadBalancer => (BaseLoadBalancerDTO) _mapper.Map<ApplicationLoadBalancerDTO>(e),
                NetworkLoadBalancer => (BaseLoadBalancerDTO) _mapper.Map<NetworkLoadBalancerDTO>(e),
                _ => throw new InvalidOperationException("Invalid balancer type")
            }
        ).ToList();
    }

    public async Task<BaseLoadBalancerDTO?> GetLoadBalancer(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(Id);
        return entity switch
        {
            ApplicationLoadBalancer => _mapper.Map<ApplicationLoadBalancerDTO>(entity),
            NetworkLoadBalancer => _mapper.Map<NetworkLoadBalancerDTO>(entity),
            _ => throw new InvalidOperationException("Invalid balancer type")
        };
    }

    public async Task<BaseLoadBalancerDTO> AddLoadBalancer(BaseLoadBalancerDTO balancer)
    {
        BaseLoadBalancer entity;
        if (balancer is ApplicationLoadBalancerDTO appBalancer)
            entity = _mapper.Map<ApplicationLoadBalancer>(appBalancer);
        else if (balancer is ApplicationLoadBalancerDTO netBalancer)
            entity = _mapper.Map<ApplicationLoadBalancer>(netBalancer);
        else
            throw new ArgumentException("Invalid balancer type");

        await _dbContext.LoadBalancers.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return _mapper.Map<BaseLoadBalancerDTO>(entity);
    }

    public async Task<BaseLoadBalancerDTO> UpdateLoadBalancer(BaseLoadBalancerDTO balancer)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        entity = _mapper.Map(balancer, entity);

        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
        
        return _mapper.Map<BaseLoadBalancerDTO>(entity);
    }

    public async Task DeleteLoadBalancer(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(Id);
        _dbContext.LoadBalancers.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }


    public async Task<List<RuleDTO>> GetRules(ApplicationLoadBalancerDTO balancer)
    {
        var entity = (ApplicationLoadBalancer?) await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        return _mapper.Map<List<RuleDTO>>(entity.Rules);
    }

    public async Task AddRule(ApplicationLoadBalancerDTO balancer, RuleDTO rule)
    {
        var entity = (ApplicationLoadBalancer?) await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        entity.Rules.Add(_mapper.Map<Rule>(rule));
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveRule(ApplicationLoadBalancerDTO balancer, RuleDTO rule)
    {
        var entity = (ApplicationLoadBalancer?) await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        var r = entity.Rules.FirstOrDefault(t => t.Id == rule.Id);
        entity.Rules.Remove(r);
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }


    public async Task<List<TargetGroupDTO>> GetTargetGroups()
    {
        var entities = await _dbContext.TargetGroups.ToListAsync();
        return _mapper.Map<List<TargetGroupDTO>>(entities);
    }

    public async Task<List<TargetGroupDTO>> GetNlbTargetGroups()
    {
        var protocols = new List<TargetGroupProtocol> {
            TargetGroupProtocol.TCP,
            TargetGroupProtocol.UDP,
            TargetGroupProtocol.TCP_UDP,
            TargetGroupProtocol.TLS
        };
        return (await GetTargetGroups()).Where(tg => protocols.Contains(tg.Protocol)).ToList();
    }

    public async Task<List<TargetGroupDTO>> GetAlbTargetGroups()
    {
        var protocols = new List<TargetGroupProtocol> {
            TargetGroupProtocol.HTTP,
            TargetGroupProtocol.HTTPS
        };
        return (await GetTargetGroups()).Where(tg => protocols.Contains(tg.Protocol)).ToList();
    }

    public async Task<TargetGroupDTO?> GetTargetGroup(Guid Id)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(Id);
        return _mapper.Map<TargetGroupDTO>(entity);
    }

    public async Task<TargetGroupDTO> AddTargetGroup(TargetGroupDTO group)
    {
        var entity = _mapper.Map<TargetGroup>(group);
        await _dbContext.TargetGroups.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return _mapper.Map<TargetGroupDTO>(entity);
    }

    public async Task<TargetGroupDTO> UpdateTargetGroup(TargetGroupDTO group)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(group.Id);
        entity = _mapper.Map(group, entity);

        _dbContext.TargetGroups.Update(entity);
        await _dbContext.SaveChangesAsync();
        
        return _mapper.Map<TargetGroupDTO>(entity);
    }

    public async Task DeleteTargetGroup(Guid Id)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(Id);
        _dbContext.TargetGroups.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<TargetDTO>> GetTargets(TargetGroupDTO group)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(group.Id);
        return _mapper.Map<List<TargetDTO>>(entity.Targets);
    }

    public async Task AddTarget(TargetGroupDTO group, TargetDTO target)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(group.Id);
        entity.Targets.Add(_mapper.Map<Target>(target));
        _dbContext.TargetGroups.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveTarget(TargetGroupDTO group, TargetDTO target)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(group.Id);
        var t = entity.Targets.FirstOrDefault(t => t.Id == target.Id);

        entity.Targets.Remove(t);
        await _dbContext.SaveChangesAsync();
    }


    public async Task ApplyConfiguration()
    {
        var balancers = await _dbContext.LoadBalancers.ToListAsync();
        await _configurator.ApplyConfiguration(balancers);
    }

    public async Task Shutdown()
    {
        await _configurator.Shutdown();
    }
}
