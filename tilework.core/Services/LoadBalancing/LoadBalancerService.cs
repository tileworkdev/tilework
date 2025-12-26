using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using AutoMapper;


using Tilework.LoadBalancing.Interfaces;
using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Enums;
using Tilework.Persistence.LoadBalancing.Models;
using Tilework.LoadBalancing.Haproxy;

using Tilework.CertificateManagement.Interfaces;
using Tilework.CertificateManagement.Models;

using Tilework.Core.Persistence;


namespace Tilework.LoadBalancing.Services;


public class LoadBalancerService : ILoadBalancerService
{
    private readonly TileworkContext _dbContext;
    private readonly LoadBalancerConfiguration _settings;
    private readonly ILoadBalancingConfigurator _configurator;
    private readonly ILogger<LoadBalancerService> _logger;
    private readonly IMapper _mapper;
    private readonly MonitoringService _monitoringService;


    public LoadBalancerService(IServiceProvider serviceProvider,
                               TileworkContext dbContext,
                               IMapper mapper,
                               IOptions<LoadBalancerConfiguration> settings,
                               ILogger<LoadBalancerService> logger,
                               MonitoringService monitoringService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
        _configurator = LoadConfigurator(serviceProvider, _settings);
        _monitoringService = monitoringService;
        _mapper = mapper;
    }

    private ILoadBalancingConfigurator LoadConfigurator(IServiceProvider serviceProvider, LoadBalancerConfiguration settings)
    {
        return settings.Backend switch
        {
            "haproxy" => serviceProvider.GetRequiredService<HAProxyConfigurator>(),
            _ => throw new ArgumentException($"Invalid configurator in load balancing tile: {_settings.Backend}")
        };
    }

    private static void ValidateRulePriority(ICollection<Rule> rules, int newPriority)
    {
        if (newPriority < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newPriority), newPriority, "Rule priority cannot be negative.");
        }

        var maxPriority = rules.Count == 0 ? -1 : rules.Max(r => r.Priority);
        if (newPriority > maxPriority + 1)
        {
            throw new ArgumentOutOfRangeException(nameof(newPriority), newPriority, $"Rule priority cannot be greater than {maxPriority + 1}.");
        }
    }

    private static bool RequiresCertificate(BaseLoadBalancer balancer)
    {
        return balancer switch
        {
            ApplicationLoadBalancer appBalancer => appBalancer.Protocol == AlbProtocol.HTTPS,
            NetworkLoadBalancer netBalancer => netBalancer.Protocol == NlbProtocol.TLS,
            _ => false
        };
    }

    private static void EnsureCertificatesPresentIfRequired(BaseLoadBalancer balancer)
    {
        if (RequiresCertificate(balancer) && (balancer.Certificates == null || balancer.Certificates.Count == 0))
        {
            throw new InvalidOperationException($"Load balancer {balancer.Name} requires at least one certificate before it can be enabled.");
        }
    }

    

    private BaseLoadBalancerDTO MapBalancerToDto(BaseLoadBalancer entity)
    {
        return entity switch
        {
            ApplicationLoadBalancer appBalancer => _mapper.Map<ApplicationLoadBalancerDTO>(appBalancer),
            NetworkLoadBalancer netBalancer => _mapper.Map<NetworkLoadBalancerDTO>(netBalancer),
            _ => throw new InvalidOperationException("Invalid balancer type")
        };
    }

    private BaseLoadBalancer MapDtoToBalancer(BaseLoadBalancerDTO dto, BaseLoadBalancer? entity = null)
    {
        return dto switch
        {
            ApplicationLoadBalancerDTO appBalancer =>
                entity == null ? _mapper.Map<ApplicationLoadBalancer>(appBalancer) : _mapper.Map(appBalancer, (ApplicationLoadBalancer)entity),

            NetworkLoadBalancerDTO netBalancer =>
                entity == null ? _mapper.Map<NetworkLoadBalancer>(netBalancer) : _mapper.Map(netBalancer, (NetworkLoadBalancer)entity),

            _ => throw new InvalidOperationException("Invalid balancer type")
        };
    }

    public async Task<List<BaseLoadBalancerDTO>> GetLoadBalancers()
    {
        var entities = await _dbContext.LoadBalancers.ToListAsync();
        return entities.Select(e => MapBalancerToDto(e)).ToList();
    }

    public async Task<BaseLoadBalancerDTO?> GetLoadBalancer(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(Id);
        return entity != null ? MapBalancerToDto(entity) : null;
    }

    public async Task<BaseLoadBalancerDTO> AddLoadBalancer(BaseLoadBalancerDTO balancer)
    {
        var entity = MapDtoToBalancer(balancer);

        await _dbContext.LoadBalancers.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return MapBalancerToDto(entity);
    }

    public async Task<BaseLoadBalancerDTO> UpdateLoadBalancer(BaseLoadBalancerDTO balancer)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        entity = MapDtoToBalancer(balancer, entity);

        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();

        return MapBalancerToDto(entity);
    }

    public async Task DeleteLoadBalancer(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(Id);
        _dbContext.LoadBalancers.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task EnableLoadBalancer(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers
                                     .Include(lb => lb.Certificates)
                                     .FirstOrDefaultAsync(lb => lb.Id == Id);

        if (entity == null)
            throw new ArgumentException($"Load balancer {Id} not found.");

        EnsureCertificatesPresentIfRequired(entity);
        entity.Enabled = true;
        _dbContext.LoadBalancers.Update(entity);

        await using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            await _dbContext.SaveChangesAsync();
            await ApplyConfiguration(Id);
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task DisableLoadBalancer(Guid Id)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(Id);
        
        entity.Enabled = false;
        _dbContext.LoadBalancers.Update(entity);

        await using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            await _dbContext.SaveChangesAsync();
            await ApplyConfiguration(Id);
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }


    public async Task<List<RuleDTO>> GetRules(ApplicationLoadBalancerDTO balancer)
    {
        var entity = (ApplicationLoadBalancer?)await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        return _mapper.Map<List<RuleDTO>>(entity.Rules.OrderBy(r => r.Priority));
    }

    public async Task AddRule(ApplicationLoadBalancerDTO balancer, RuleDTO rule)
    {
        var entity = (ApplicationLoadBalancer?)await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        if (entity == null)
            throw new ArgumentNullException(nameof(balancer));

        ValidateRulePriority(entity.Rules, rule.Priority);
        
        foreach (var existingRule in entity.Rules.Where(r => r.Priority >= rule.Priority))
        {
            existingRule.Priority += 1;
        }

        entity.Rules.Add(_mapper.Map<Rule>(rule));
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateRule(ApplicationLoadBalancerDTO balancer, RuleDTO rule)
    {
        var entity = (ApplicationLoadBalancer?)await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        if (entity == null)
            throw new ArgumentNullException(nameof(balancer));

        var ruleEntity = entity.Rules.FirstOrDefault(t => t.Id == rule.Id);
        if (ruleEntity == null)
            return;

        ValidateRulePriority(entity.Rules, rule.Priority);

        await using var tx = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var newPriority = rule.Priority;
            var originalPriority = ruleEntity.Priority;
                
            if (newPriority != originalPriority)
            {
                // first move the original rule out of the way in order to not trip a constraint error
                ruleEntity.Priority = -1;
                _dbContext.Entry(ruleEntity).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();


                if (newPriority < originalPriority)
                {
                    // rule moved up. Bump down rules from new priority to old priority
                    var rulesToAdjust = entity.Rules.Where(r => r.Id != rule.Id &&
                                                                r.Priority >= newPriority &&
                                                                r.Priority < originalPriority)
                                                    .OrderByDescending(r => r.Priority);

                    foreach (var ruleToAdjust in rulesToAdjust)
                    {
                        ruleToAdjust.Priority += 1;
                        _dbContext.Entry(ruleToAdjust).State = EntityState.Modified;
                        await _dbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    // rule moved down. Bump up rules from new priority to old priority
                    var rulesToAdjust = entity.Rules.Where(r => r.Id != rule.Id &&
                                                                r.Priority <= newPriority &&
                                                                r.Priority > originalPriority)
                                                    .OrderBy(r => r.Priority);

                    foreach (var ruleToAdjust in rulesToAdjust)
                    {
                        ruleToAdjust.Priority -= 1;
                        _dbContext.Entry(ruleToAdjust).State = EntityState.Modified;
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }

            var moddedRules = entity.Rules.OrderBy(r => r.Priority);

            _mapper.Map(rule, ruleEntity);
            _dbContext.Entry(ruleEntity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task RemoveRule(ApplicationLoadBalancerDTO balancer, RuleDTO rule)
    {
        var entity = (ApplicationLoadBalancer?)await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        if (entity == null)
            throw new ArgumentNullException();

        var ruleEntity = entity.Rules.FirstOrDefault(t => t.Id == rule.Id);
        if (ruleEntity == null)
            return;

        var pri = ruleEntity.Priority;
        entity.Rules.Remove(ruleEntity);

        // bump up lower rules
        foreach (var lowerRule in entity.Rules.Where(r => r.Priority > pri))
        {
            lowerRule.Priority -= 1;
        }
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<CertificateDTO>> GetCertificates(BaseLoadBalancerDTO balancer)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        return _mapper.Map<List<CertificateDTO>>(entity.Certificates);
    }

    public async Task AddCertificate(BaseLoadBalancerDTO balancer, Guid certificateId)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        var cert = await _dbContext.Certificates.FindAsync(certificateId);
        entity.Certificates.Add(cert);
        _dbContext.LoadBalancers.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveCertificate(BaseLoadBalancerDTO balancer, Guid certificateId)
    {
        var entity = await _dbContext.LoadBalancers.FindAsync(balancer.Id);
        var c = entity.Certificates.FirstOrDefault(t => t.Id == certificateId);
        entity.Certificates.Remove(c);
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

    public async Task UpdateTarget(TargetGroupDTO group, TargetDTO target)
    {
        var entity = await _dbContext.TargetGroups.FindAsync(group.Id);
        var t = entity.Targets.FirstOrDefault(t => t.Id == target.Id);
        if (t != null)
        {
            _mapper.Map(target, t);
            _dbContext.TargetGroups.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
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

    public async Task ApplyConfiguration(Guid Id)
    {
        var balancer = await _dbContext.LoadBalancers.FindAsync(Id);
        if(balancer != null)
            await _configurator.ApplyConfiguration(balancer);
    }

    public async Task Shutdown()
    {
        await _configurator.Shutdown();
    }

    public async Task<List<LoadBalancingMonitorData>> GetLoadBalancerMonitoringData(Guid id, TimeSpan interval, DateTimeOffset start, DateTimeOffset end)
    {
        var lb = await GetLoadBalancer(id);
        if(lb == null)
            throw new ArgumentException("Invalid load balancer id");

        var filters = new Dictionary<string, string>();
        filters["instance"] = lb.Id.ToString();
        filters["type"] = "frontend";
        filters["proxy"] = lb.Id.ToString();

        return await _monitoringService.GetMonitoringData<LoadBalancingMonitorData>("LoadBalancing", filters, interval, start, end);
    }
}
